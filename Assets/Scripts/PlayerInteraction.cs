using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteraction : MonoBehaviour {
    // A reference to the interaction text object
    public GameObject interactionPrompt;

    private Interactable interactableInRange;

    public void Start() {
        // Check for a null reference to the interaction prompt
        if (interactionPrompt != null) {
            // If its not null ensure its disabled initially
            interactionPrompt.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other) {
        // Get a reference to the interactable component of the other object
        Interactable interactable = other.GetComponent<Interactable>();
        
        // Check for a null reference
        if (interactable != null) {
            // Store the reference to the in range interactable
            interactableInRange = interactable;

            // Check if the interaction prompt has a valid ref
            if (interactionPrompt != null) {
                // If it does enable the interact prompt
                interactionPrompt.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other) {
        // Get a reference to the interactable component of the other object
        Interactable interactable = other.GetComponent<Interactable>();

        // Check if this was the interactable that was triggered
        if (interactable != null && interactable == interactableInRange) {
            interactableInRange = null; // reset its ref

            // Check for a valid ref to the interaction prompt
            if (interactionPrompt != null) {
                // If valid, disable the interaction prompt
                interactionPrompt.SetActive(false);
            }
        }
    }

    private void Update() {
        // Trigger the interaction for the in range interactable
        if (interactableInRange != null && Input.GetKeyDown(KeyCode.E)) {
            interactableInRange.Interact();
        }
    }
}
