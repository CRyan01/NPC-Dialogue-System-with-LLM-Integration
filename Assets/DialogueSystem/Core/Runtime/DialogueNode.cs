using System;

namespace DialogueSystem.Core {
    // Represents a single step (node) in a conversation (dialogue tree).
    [Serializable]
    public class DialogueNode {
        // Unique within the conversation.
        public string id;

        // Who is speaking? (NPC, Player, etc.).
        public string speaker;

        // The dialouge text shown for this node.
        public string text;

        // The choices the player can select from this node.
        // Can be null or empty for no choices.
        public DialogueChoice[] choices;
    }
}
