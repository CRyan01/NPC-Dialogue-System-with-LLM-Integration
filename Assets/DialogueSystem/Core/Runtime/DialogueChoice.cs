using System;

namespace DialogueSystem.Core {
    // Represents a single selectable option from a dialogue node.
    [Serializable]
    public class DialogueChoice {
        // The text shown to the player for this choice.
        public string text;

        // The ID of the node to go to if this choice is selected.
        public string nextNodeId;
    }
}
