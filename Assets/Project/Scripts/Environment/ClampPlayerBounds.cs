using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClampPlayerBounds : MonoBehaviour {
    // To store a reference to the ground object.
    private Transform ground;
    // The center point of the ground plane.
    private Vector3 groundCenter;
    // Half of the grounds width.
    private float halfWidth;
    // Half of the grounds height.
    private float halfLength;

    void Start() {
        // Find and store a ref to the ground object.
        ground = GameObject.FindWithTag("Ground").transform;

        // Get the grounds bounds.
        Renderer groundRenderer = ground.GetComponent<Renderer>();
        Bounds groundBounds = groundRenderer.bounds;

        // Store the halfway value for the ground x and z axis.
        halfWidth = groundBounds.extents.x;
        halfLength = groundBounds.extents.z;

        // Store the grounds center point.
        groundCenter = groundBounds.center;
    }

    void LateUpdate() {
        // Get the actors current position.
        Vector3 currentPosition = transform.position;

        // Clamp the actors position.
        currentPosition.x = Mathf.Clamp(
            currentPosition.x,
            groundCenter.x - halfWidth,
            groundCenter.x + halfWidth
            );

        currentPosition.z = Mathf.Clamp(
            currentPosition.z,
            groundCenter.z - halfLength,
            groundCenter.z + halfLength
            );

        // Apply the clamped position to the actor
        transform.position = currentPosition;
    }
}
