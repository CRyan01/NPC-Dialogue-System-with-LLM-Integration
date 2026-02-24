using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DialogueSystem.Core;
using DialogueSystem.Unity;
using System;

// UI controller which listens to DialogueService events and updates the on-screen dialogue panels.
// When a conversation starts -> show the NPC panel first, when a node is entered -> show NPC speaker & response.
// If the player clicks anywhere on-screen -> switch to the choices panel.
// The choices panel shows a vertical list of choice buttons.
// When a conversation ends -> hide everything.
// Clicking a choice calls DialogueService.Choose(index).
public class DialogueUI : MonoBehaviour {
    [SerializeField] private GameObject npcPanel; // panel that shows NPC text.
    [SerializeField] private GameObject choicePanel; // panel that shows player dialogue choices.

    [SerializeField] private TMP_Text speakerText; // text to show the speakers name.
    [SerializeField] private TMP_Text bodyText; // text to show a response.

    [SerializeField] private Transform choicesContainter; // parent container for choice buttons.
    [SerializeField] private Button choiceButtonPrefab; // prefab for a choice selection button.

    [SerializeField] private DialogueSystem.Unity.LLMService llmService;

    private string m_lastPlayerChoiceText;
    private bool m_shouldGenerateNPCReply;
    private bool m_isGenerating;

    // Simple state machine for the UI.
    private enum DialogueUIState {
        Hidden, // nothing visible.
        NpcSpeaking, // NPC panel shown (waiting for click).
        PlayerChoosing // choices displayed on-screen.
    }
    
    // Current state.
    private DialogueUIState m_state = DialogueUIState.Hidden;
    // Node currently being displayed.
    private DialogueNode m_currentNode;

    private void Awake() {
        // Start with all dialogue UI hidden.
        SetPanelsActive(false, false);
    }

    private void Start() {
        // Check if there is a valid DialogueService instance.
        if (DialogueService.Instance == null) {
            Debug.LogError("DialogueUI: DialogueService.Instance is null. Make sure DialogueService is in the scene.");
            return;
        }

        // Subscribe to events raised by DialogueService.
        DialogueService.Instance.OnConversationStarted += HandleConversationStarted;
        DialogueService.Instance.OnConversationEnded += HandleConversationEnded;
        DialogueService.Instance.OnNodeEntered += HandleNodeEntered;
        DialogueService.Instance.OnChoiceSelected += HandleChoiceSelected;
    }

    private void OnDisable() {
        // Check if there is a valid DialogueService instance.
        if (DialogueService.Instance == null) {
            return;
        }

        // Unsubscribe to events raised by DialogueService.
        DialogueService.Instance.OnConversationStarted -= HandleConversationStarted;
        DialogueService.Instance.OnConversationEnded -= HandleConversationEnded;
        DialogueService.Instance.OnNodeEntered -= HandleNodeEntered;
        DialogueService.Instance.OnChoiceSelected -= HandleChoiceSelected;
    }

    private void Update() {
        // Dont allow selection while already generating.
        if (m_isGenerating) {
            return;
        }

        // Mouse click anywhere to advance from NPC speaking to choices.
        if (m_state == DialogueUIState.NpcSpeaking && Input.GetMouseButtonDown(0)) {
            ShowChoicesForCurrentNode();
        }
    }

    private void HandleConversationStarted(DialogueConversation conversation) {
        // When a conversation starts, show the NPC panel first.
        m_state = DialogueUIState.NpcSpeaking;
        SetPanelsActive(npcActive: true, choicesActive: true);
        
        // Unlock the cursor and make it visible.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void HandleConversationEnded(DialogueConversation conversation) {
        // Cleanup UI when the conversation ends.
        m_state = DialogueUIState.Hidden;
        m_currentNode = null;

        SetPanelsActive(false, false);
        ClearChoices();
        SetNpcTexts(string.Empty, string.Empty);

        // Lock the cursor and make it invisible.
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private async void HandleNodeEntered(DialogueNode node) {
        // Check if the node is valid.
        if (node == null) {
            return;
        }

        // Store the reference.
        m_currentNode = node;

        // Show NPC panel with this nodes speaker + text.
        m_state = DialogueUIState.NpcSpeaking;
        SetPanelsActive(npcActive: true, choicesActive: false);

        SetNpcTexts(node.speaker, node.text);
        ClearChoices();

        // Generate for NPC lines after a player choice.
        if (m_shouldGenerateNPCReply && node.speaker == "NPC" && llmService != null && !m_isGenerating) {
            m_shouldGenerateNPCReply = false;
            m_isGenerating = true;

            SetNpcTexts(node.speaker, "Generating...");

            try {
                string reply = await llmService.GenerateNPCReplyAsync(
                    m_lastPlayerChoiceText, // player choice line.
                    node.text // canon npc line.
                    );

                SetNpcTexts(node.speaker, reply);
            } catch (Exception ex) {
                Debug.LogWarning("LLM generation failed. Using fallback. " + ex.Message);

                // Fallback to original line.
                SetNpcTexts(node.speaker, node.text);
            } finally {
                m_isGenerating = false;
            }
        }
    }

    private void HandleChoiceSelected(DialogueNode node, int choiceIndex) {
        // Check if the node is valid.
        if (node == null) {
            return;
        }

        // Check if the node has valid choices.
        if (node.choices == null) {
            return;
        }

        // Check if the players selected choice is valid.
        if (choiceIndex < 0 || choiceIndex >= node.choices.Length) {
            return;
        }
        
        // Store the selected choice text.
        m_lastPlayerChoiceText = node.choices[choiceIndex].text;
        m_shouldGenerateNPCReply = true;
    }

    private void SetPanelsActive(bool npcActive, bool choicesActive) {
        // Toggle NPC panel.
        if (npcPanel != null) {
            npcPanel.SetActive(npcActive);
        }

        // Toggle choice panel.
        if (choicePanel != null) {
            choicePanel.SetActive(choicesActive);
        }
    }

    private void SetNpcTexts(string speaker, string body) {
        // Update speaker label.
        if (speakerText != null) {
            speakerText.text = speaker;
        }

        // Update dialogue text.
        if (bodyText != null) {
            bodyText.text = body;
        }
    }

    private void ClearChoices() {
        // Check for null reference to the container.
        if (choicesContainter == null) {
            return;
        }

        // Remove all choice buttons from the container.
        for (int i = choicesContainter.childCount - 1; i >= 0; i--) {
            Destroy(choicesContainter.GetChild(i).gameObject);
        }
    }

    private void ShowChoicesForCurrentNode() {
        // Check if there is a valid reference to a node.
        if (m_currentNode == null) {
            return;
        }

        // If there are no choices at this node, just stay on the NPC panel.
        if (m_currentNode.choices == null || m_currentNode.choices.Length == 0) {
            return;
        }

        // Enter PlayerChoosing state.
        m_state = DialogueUIState.PlayerChoosing;
        SetPanelsActive(npcActive: false, choicesActive: true);

        ClearChoices();

        // Ensure references were properly set.
        if (choicesContainter == null || choiceButtonPrefab == null) {
            Debug.LogError("DialogueUI: Choices container or button prefab was not assigned.");
            return;
        }
        
        // Contruct buttons for each choice.
        for (int i = 0; i < m_currentNode.choices.Length; i++) {
            int choiceIndex = i;
            DialogueChoice choice = m_currentNode.choices[choiceIndex];

            // Instansiate buttons inside the choices container.
            Button button = Instantiate(choiceButtonPrefab, choicesContainter);

            // Find and set the label text inside the button.
            TMP_Text label = button.GetComponentInChildren<TMP_Text>();
            if (label != null) {
                label.text = choice.text;
            }

            // Register the click handler.
            button.onClick.AddListener(() => {
                // When a choice is clicked, signal to DialogueService.
                DialogueService.Instance.Choose(choiceIndex);
            });
        }
    }
}
