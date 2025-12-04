using System;

namespace DialogueSystem.Core {
    // Represents a complete conversation (dialogue tree).
    [Serializable]
    public class DialogueConversation {
        // Unique conversation identifier (e.g. "npc_merchant_intro").
        public string id;

        // ID of the node to start at (e.g. "start").
        public string startNodeId;

        // All nodes in this conversation.
        public DialogueNode[] nodes;
    }
}