using DialogueSystem.Unity;
using UnityEngine;

public class Interactable : MonoBehaviour {
    // Text to display when in range.
    public string interactionText = "Press E to interact";

    // ID of the conversation to start when this object is interacted with.
    public string conversationId = "npc_intro";

    public void Interact() {
        // What happens when an interaction is triggered.
        Debug.Log("Interacted with " + gameObject.name);

        // Try to start the conversation if DialogueService is available.
        if (DialogueService.Instance == null) {
            Debug.LogError("Interactable: DialogueService.Instance is null. Make sure DialogueService is in the scene.");
            return;
        }

        // Check if the contents of conversationId are valid.
        if (string.IsNullOrEmpty(conversationId)) {
            Debug.LogWarning("Interactable: conversationId is empty on gameObject " + gameObject.name);
            return;
        }

        // Try to start a conversation.
        bool conversationStarted = DialogueService.Instance.TryStartConversation(conversationId);

        // Check if the conversation successfully started.
        if (!conversationStarted) {
            Debug.LogWarning("Interactable: Failed to start conversation with id: " + conversationId);
        }
    }
}
