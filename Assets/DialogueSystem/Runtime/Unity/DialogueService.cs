using System;
using UnityEngine;
using DialogueSystem.Core;

namespace DialogueSystem.Unity {
    // Unity-facing wrapper for DialogueRuntime. Acts as the main entry point for other Unity scripts.
    // Holds a single DialogueRuntime instance, loads a DialogueDatabase from a JSON TextAsset.
    // Provides public methods for starting conversations and choosing options.
    // Re-exposes DialogueRuntime events so other scripts can subscribe.
    public class DialogueService : MonoBehaviour {
        // Global access point for the DialogueService instance in the scene.
        // e.g. DialogueService.Instance.TryStartConversation("npc_intro");
        // There should be only one instance in the scene.
        public static DialogueService Instance { get; private set; }

        // JSON TextAsset which contains the dialogue database.
        // Contents should match the DialogueDatabase structure.
        // e.g. (conversations array -> nodes -> choices).
        [SerializeField] private TextAsset m_jsonDatabase;

        // If true, the JSON database will automatically be loaded
        // and DialogueRuntime will be created during Awake()
        [SerializeField] private bool m_autoInitialize = true;

        // If true, the DialogueService GameObject will not be destroyed
        // when loading a new scene.
        [SerializeField] private bool m_dontDestroyOnLoad = true;

        // Prevents double initialization of the database.
        private bool m_initialized = false;

        // The core dialogue runtime that handles all of the logic.
        // Created from the loaded DialogueDatabase.
        public DialogueRuntime Runtime { get; private set; }

        // The DialogueDatabase loaded from the JSON.
        public DialogueDatabase Database { get; private set; }


        // Fired when a conversation starts. Forwarded from Runtime.OnConversationStarted.
        public event Action<DialogueConversation> OnConversationStarted;

        // Fired when a conversation ends. Forwarded from Runtime.OnConversationEnded.
        public event Action<DialogueConversation> OnConversationEnded;

        // Fired when the runtime enters a new node. Forwarded from Runtime.OnNodeEntered.
        public event Action<DialogueNode> OnNodeEntered;

        // Fired when a choice is selected. Forwarded from Runtime.OnChoiceSelected.
        public event Action<DialogueNode, int> OnChoiceSelected;

        private void Awake() {
            // Singleton pattern.
            // If there is an existing instance and it isn't this one, destroy it.
            // Otherwise, assign Instance to this.
            if (Instance != null && Instance != this) {
                Debug.LogWarning("DialogueService: Another instance already exists. Destroying duplicate DialogueService on GameObject: " + gameObject.name);
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (m_dontDestroyOnLoad) {
                DontDestroyOnLoad(gameObject);
            }

            if (m_autoInitialize) {
                InitializeFromJsonAsset();
            }
        }

        // Initializes the DialogueService using the JSON TextAsset.
        // Can be called automatically on Awake() or manually.
        public void InitializeFromJsonAsset() {
            if (m_initialized) {
                Debug.LogWarning("DialogueService: InitializeFromJsonAsset was already called. Skipping re-initialization.");
                return;
            }

            if (m_jsonDatabase == null) {
                Debug.LogError("DialogueService: No JSON database assigned. Please assign a TextAsset.");
                return;
            }

            // Deserialize the JSON text into a DialogueDatabase instance.
            // Requires the JSON structure to match the C# classes.
            DialogueDatabase loadedDatabase = null;

            try {
                loadedDatabase = JsonUtility.FromJson<DialogueDatabase>(m_jsonDatabase.text);
            } catch (Exception ex) {
                Debug.LogError("DialogueService: Failed to deserialize JSON database. Exception: " + ex.Message);
                return;
            }

            if (loadedDatabase == null) {
                Debug.LogError("DialogueService: Deserialized DialogueDatabase is null. Check JSON format.");
                return;
            }

            if (loadedDatabase.conversations == null || loadedDatabase.conversations.Length == 0) {
                Debug.LogWarning("DialogueService: Database contains no conversations. Check JSON content.");
            }

            // Store the database for reference
            Database = loadedDatabase;

            // Create/re-create the DialogueRuntime with this database.
            CreateRuntime(Database);

            // Mark the database as initialized.
            m_initialized = true;
        }

        // Creates a new DialogueRuntime from the given DialogueDatabase
        // and hooks up event forwarding.
        public void CreateRuntime(DialogueDatabase database) {
            if (database == null) {
                throw new ArgumentNullException("database", "DialogueService.CreateRuntime was given a null database.");
            }

            if (Runtime != null) {
                UnsubscribeFromRuntimeEvents(Runtime);
            }

            // Create a new Runtime with the provided database.
            Runtime = new DialogueRuntime(database);

            // Subscribe to runtime events so they can be forwarded.
            SubscribeToRuntimeEvents(Runtime);
        }

        // Returns true if there is an active conversation running.
        public bool IsConversationActive {
            get {
                return Runtime != null && Runtime.IsActive;
            }
        }

        // Try to start a conversation by its ID.
        // Forwards the call to Runtime.TryStartConversation.
        // Returns true if the conversation with this ID exists and was started, otherwise false.
        public bool TryStartConversation(string conversationId) {
            if (Runtime == null) {
                Debug.LogError("DialogueService: Cannot start conversation. Runtime is null. Ensure InitializeFromJsonAsset() has been called.");
                return false;
            }

            return Runtime.TryStartConversation(conversationId);
        }

        // Choose one of the available options at the current node.
        // Forwards the call to Runtime.Choose.
        public void Choose(int choiceIndex) {
            if (Runtime == null) {
                Debug.LogError("DialogueService: Cannot choose. Runtime is null. Ensure InitializeFromJsonAsset() has been called.");
                return;
            }

            Runtime.Choose(choiceIndex);
        }

        // Ends the current conversation, if there is one.
        // Forwards the call to Runtime.EndConversation.
        public void EndConversation() {
            if (Runtime == null) {
                Debug.LogError("DialogueService: Cannot end conversation. Runtime is null. Ensure InitializeFromJsonAsset() has been called.");
                return;
            }

            Runtime.EndConversation();
        }

        // Subscribe this service to the runtime's events,
        // and forward them to new events.
        private void SubscribeToRuntimeEvents(DialogueRuntime runtime) {
            if (runtime == null) {
                Debug.LogWarning("DialogueService: SubscribeToRuntimeEvents was given a null runtime.");
                return;
            }

            runtime.OnConversationStarted += HandleConversationStarted;
            runtime.OnConversationEnded += HandleConversationEnded;
            runtime.OnNodeEntered += HandleNodeEntered;
            runtime.OnChoiceSelected += HandleChoiceSelected;
        }

        // Unsubscribe this service from the runtime's events.
        // Must be called before replacing or destroying a runtime.
        private void UnsubscribeFromRuntimeEvents(DialogueRuntime runtime) {
            if (runtime == null) {
                Debug.LogWarning("DialogueService: UnsubscribeFromRuntimeEvents was given a null runtime.");
                return;
            }

            runtime.OnConversationStarted -= HandleConversationStarted;
            runtime.OnConversationEnded -= HandleConversationEnded;
            runtime.OnNodeEntered -= HandleNodeEntered;
            runtime.OnChoiceSelected -= HandleChoiceSelected;
        }

        private void HandleConversationStarted(DialogueConversation conversation) {
            // Forward to DialogueService-level event
            if (OnConversationStarted != null) {
                OnConversationStarted(conversation);
            }
        }

        private void HandleConversationEnded(DialogueConversation conversation) {
            // Forward to DialogueService-level event
            if (OnConversationEnded != null) {
                OnConversationEnded(conversation);
            }
        }

        private void HandleNodeEntered(DialogueNode node) {
            // Forward to DialogueService-level event
            if (OnNodeEntered != null) {
                OnNodeEntered(node);
            }
        }

        private void HandleChoiceSelected(DialogueNode node, int choiceIndex) {
            // Forward to DialogueService-level event
            if (OnChoiceSelected != null) {
                OnChoiceSelected(node, choiceIndex);
            }
        }

        private void OnDestroy() {
            if (Runtime != null) {
                UnsubscribeFromRuntimeEvents(Runtime);
            }

            if (Instance == this) {
                Instance = null;
            }
        }
    }
}
