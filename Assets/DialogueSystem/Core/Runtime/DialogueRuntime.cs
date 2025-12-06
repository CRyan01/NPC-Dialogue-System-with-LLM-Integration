using System;
using System.Collections.Generic;

namespace DialogueSystem.Core {
    // The core dialogue engine.
    // When given a DialogueDatabase, build lookup tables for conversation & nodes, start conversations by ID,
    // move between novdes based on choices, and raise events so other systems can react.

    public class DialogueRuntime {
        // Public events (event API).
        public event Action<DialogueConversation> OnConversationStarted; // Fired when a conversation starts.
        public event Action<DialogueConversation> OnConversationEnded; // Fired when a conversation ends.
        public event Action<DialogueNode> OnNodeEntered; // Fired when a entering a new node.
        public event Action<DialogueNode, int> OnChoiceSelected; // Fired when the players picks a choice at the current node.

        // Dialogue database (all conversations + all nodes).
        private DialogueDatabase m_database;

        // Dictionary for looking up a conversation by its string ID, e.g. "npc_intro".
        private Dictionary<string, DialogueConversation> m_conversationsById;

        // Dictionary for looking up a node by a composite key "conversationId:nodeId" e.g. "npc_intro:start".
        private Dictionary<string, DialogueNode> m_nodesByKey;

        // The conversation that is currently running.
        private DialogueConversation m_currentConversation;

        // The node that is currently active in the current conversation.
        private DialogueNode m_currentNode;
        
        // Exposes the current conversation (read-only).
        public DialogueConversation CurrentConversation {
            get { return m_currentConversation; }
        }
        // Exposes the current node (read-only).
        public DialogueNode CurrentNode {
            get { return m_currentNode; }
        }
        // Returns true if there is a conversation currently running.
        public bool IsActive {
            get { return m_currentConversation != null; }
        }

        // Constructor which needs a DialogueDatabase.
        // When the runtime is created, it loads and indexes the database.
        public DialogueRuntime(DialogueDatabase database) {
            LoadDatabase(database);
        }

        // Load or reload the dialogue database and rebuild lookup structures.
        // Must be called before conversations can be started.
        public void LoadDatabase(DialogueDatabase database) {
            // Check if a null database was passed in.
            if (database == null) {
                throw new ArgumentNullException(nameof(database));
            }

            // Store the database reference.
            m_database = database;

            // Create new dictionaries for conversations.
            m_conversationsById = new Dictionary<string, DialogueConversation>();
            m_nodesByKey = new Dictionary<string, DialogueNode>();

            // Reset the current state.
            m_currentConversation = null;
            m_currentNode = null;

            // If there are no conversations defined yet, return as theres nothing to build.
            if (m_database.conversations == null) {
                return;
            }

            // Loop through all conversations in the database.
            for (int i = 0; i < m_database.conversations.Length; i++) {
                // Select the conversation at the current index.
                DialogueConversation conversation = m_database.conversations[i];

                // Skip null entries
                if (conversation == null) {
                    continue;
                }

                // Store this conversation in the dictionary by its ID.
                // e.g. m_conversationsById["npc_intro"] = conversation;
                m_conversationsById[conversation.id] = conversation;

                // If this conversation has no nodes, skip building node lookup.
                if (conversation.nodes == null) {
                    continue;
                }

                // Loop over all nodes in this conversation.
                for (int j = 0; j < conversation.nodes.Length; j++) {
                    // Select the node at the current index.
                    DialogueNode node = conversation.nodes[j];

                    // Skip null nodes or nodes without a valid ID.
                    if (node == null || string.IsNullOrEmpty(node.id)) { 
                        continue; 
                    }

                    // Build a unique key for this node, based on conversation ID and node ID
                    // e.g. "npc_intro:start"
                    string nodeKey = MakeNodeKey(conversation.id, node.id);

                    // Store this node in the dictionary so it can be found easily later.
                    m_nodesByKey[nodeKey] = node;
                }
            }
        }

        // Try to start a conversation by ID.
        // Returns true if the conversation exists and was started, otherwise returns false.
        public bool TryStartConversation(string conversationId) {
            // Check if an invalid ID was passed.
            if (string.IsNullOrEmpty(conversationId)) {
                throw new ArgumentException("Conversation ID cannot be null or empty.", "conversationId");
            }

            // Check if the conversation ID exists in the dictionary, if it doesn't it can't be started.
            if (!m_conversationsById.ContainsKey(conversationId)) {
                return false;
            }

            // Set the current conversation to the one found.
            m_currentConversation = m_conversationsById[conversationId];

            // Clear any previous node, a new conversation starts at its start node.
            m_currentNode = null;

            // Notify listeners that a conversation has started.
            if (OnConversationStarted != null) {
                OnConversationStarted(m_currentConversation);
            }

            // Move to conversations starting node.
            // This should raise OnNodeEntered.
            GoToNode(m_currentConversation.startNodeId);
            return true;
        }

        // Choose one of the available choices at the current node.
        // choiceIndex is the index in m_currentNode.choices.
        public void Choose(int choiceIndex) {
            // If no conversation is active or there's no current node, do nothing.
            if (!IsActive || m_currentNode == null) {
                return;
            }

            // Get an array of choices for this node.
            DialogueChoice[] choices = m_currentNode.choices;

            // If there are no choices defined, there's nothing to choose.
            if (choices == null || choices.Length == 0) {
                return;
            }

            // If the provided index is out of bounds, do nothing.
            if (choiceIndex < 0 || choiceIndex >= choices.Length) {
                return;
            }

            // Notify listeners that a choice was selected.
            if (OnChoiceSelected != null) {
                OnChoiceSelected(m_currentNode, choiceIndex);
            }

            // Get the chosen choice.
            DialogueChoice choice = choices[choiceIndex];

            // Get the ID of the next node to go to.
            string nextId = choice.nextNodeId;

            // If nextId is null or empty, or equals "end", end the conversation.
            if (string.IsNullOrEmpty(nextId) || nextId.Equals("end", StringComparison.OrdinalIgnoreCase)) {
                EndConversation();
            } else {
                // Otherwise move to the node with ID = nextID.
                GoToNode(nextId);
            }
        }

        // Forcefully end the current conversation, if one exists.
        public void EndConversation() {
            // If there's no active conversation, do nothing.
            if (!IsActive) {
                return;
            }

            // Store the conversation before ending it, so it can be passed to the OnConversationEnded event.
            DialogueConversation endedConversation = m_currentConversation;

            // Clear current state.
            m_currentConversation = null;
            m_currentNode = null;

            // Notify listeners that the conversation has ended.
            if (OnConversationEnded != null) {
                OnConversationEnded(endedConversation);
            }
        }

        // Helper to move to a specific node ID within the current conversation.
        // If the node can't be found, the conversation ends.
        public void GoToNode(string nodeId) {
            // Check if the conversation is active, and that the node ID is valid before continuing.
            if (!IsActive || string.IsNullOrEmpty(nodeId)) {
                // If not end the conversation and return.
                EndConversation();
                return;
            }

            // Build the composite key for this node in the current conversation.
            // e.g. "npc_intro:start"
            string nodeKey = MakeNodeKey(m_currentConversation.id, nodeId);

            // Check if the node exists in the dictionary.
            if (!m_nodesByKey.ContainsKey(nodeKey)) {
                // If the node wasn't found, end the conversation safely.
                EndConversation();
                return;
            }

            // Get and set the current node.
            m_currentNode = m_nodesByKey[nodeKey];

            // Notify listeners that a new node has been entered.
            if (OnNodeEntered != null) {
                OnNodeEntered(m_currentNode);
            }
        }

        // Helper function to build a unique key for a node, combining conversation and node IDs e.g. "npc_intro:start".
        private string MakeNodeKey(string conversationId, string nodeId) {
            return conversationId + ":" + nodeId;
        }
    }
}
