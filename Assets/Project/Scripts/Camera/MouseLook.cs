using UnityEngine;

public class MouseLook : MonoBehaviour {
    public Transform playerObject; // a reference to the player object.

    // How sensitive the camera is to mouse movement (multiplier).
    public float mouseSensitivity = 600.0f;

    // The cameras distance, and min/max camera distance from the player.
    public float cameraDistance = 6.0f;
    public float minDistance = 3.0f;
    public float maxDistance = 12.0f;

    // Min and max pitch values.
    public float minPitch = -30.0f;
    public float maxPitch = 60.0f;

    // The zoom speed of the camera.
    public float zoomSpeed = 5.0f;

    // Smooth settings
    public float zoomSmoothTime = 0.05f;
    public float rotationSmoothTime = 0.05f;

    float xRotation; // pitch.
    float yRotation; // yaw.

    // Smoothed rotation values.
    float smoothedXRotation;
    float smoothedYRotation;

    // Internal smoothing velocities.
    float xRotationVelocity;
    float yRotationVelocity;
    float zoomVelocity;

    // Smoothed zoom.
    float smoothedCameraDistance;

    void Start() {
        // Lock the cursor to the center.
        Cursor.lockState = CursorLockMode.Locked;

        // Initialize smoothed values.
        smoothedXRotation = xRotation;
        smoothedYRotation = yRotation;
        smoothedCameraDistance = cameraDistance;
    }

    void LateUpdate() {
        // Get input values from the mouse.
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        
        yRotation += mouseX; // move left or right (yaw).
        xRotation -= mouseY; // move up or down (pitch).
        xRotation = Mathf.Clamp(xRotation, minPitch, maxPitch); // clamp pitch.

        // Smooth the rotation.
        smoothedXRotation = Mathf.SmoothDampAngle(smoothedXRotation, xRotation, ref xRotationVelocity, rotationSmoothTime);
        smoothedYRotation = Mathf.SmoothDampAngle(smoothedYRotation, yRotation, ref yRotationVelocity, rotationSmoothTime);

        // Get input value from the scroll wheel.
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        // Calculate target distance based on scroll input.
        float targetDistance = cameraDistance - scrollInput * zoomSpeed;
        targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);

        // Smooth zoom movements.
        smoothedCameraDistance = Mathf.SmoothDamp(smoothedCameraDistance, targetDistance, ref zoomVelocity, zoomSmoothTime);

        // Build the final rotation.
        Quaternion rotation = Quaternion.Euler(smoothedXRotation, smoothedYRotation, 0.0f);

        // Calculate the final camera position.
        Vector3 cameraOffset = rotation * new Vector3(0.0f, 0.0f, -smoothedCameraDistance);

        // Apply the movement.
        transform.position = playerObject.position + cameraOffset;
        transform.LookAt(playerObject.position);

        // Update target distance for the next frame.
        cameraDistance = targetDistance;
    }
}
