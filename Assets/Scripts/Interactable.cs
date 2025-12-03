using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour {
    // Text to display when in range
    public string interactionText = "Press E to interact";

    public void Interact() {
        // What happens when an interaction is triggered
        Debug.Log("Interacted with " + gameObject.name);
    }
}
