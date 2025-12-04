using System;

namespace DialogueSystem.Core {
    // Container for all dialogue content. Holds an array of DialogueConversation objects.
    [Serializable]
    public class DialogueDatabase {
        // All conversations available in this database.
        public DialogueConversation[] conversations;
    }
}
