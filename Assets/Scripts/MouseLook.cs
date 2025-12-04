using UnityEngine;

public class MouseLook : MonoBehaviour {
    public Transform playerObject; // a reference to the player object


    public float mouseSensitivity = 150.0f; // a multiplier for the rotation
    public float cameraDistance = 6.0f; // the cameras distance from the player

    public float minDistance = 3.0f; // the min distance from the player
    public float maxDistance = 12.0f; // the max distance from the player
    public float zoomSpeed = 5.0f; // the zoom speed of the camera

    public float minPitch = -30.0f; // the min pitch value
    public float maxPitch = 60.0f; // the max pitch value

    float xRotation; // pitch
    float yRotation; // yaw

    void Start() {
        // Lock the cursor to the center
        Cursor.lockState = CursorLockMode.Locked;
    }

    void LateUpdate() {
        // Get input values from the mouse
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        
        yRotation += mouseX; // move left or right (yaw)
        xRotation -= mouseY; // move up or down (pitch)
        xRotation = Mathf.Clamp(xRotation, minPitch, maxPitch); // clamp pitch

        // Get input value from the scroll wheel
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");

        // Apply zoom speed multipler and clamp camera distance
        cameraDistance -= scrollInput * zoomSpeed;
        cameraDistance = Mathf.Clamp(cameraDistance, minDistance, maxDistance);

        // Build the rotation
        Quaternion rotation = Quaternion.Euler(xRotation, yRotation, 0.0f);

        // Position the camera behind the player
        Vector3 cameraOffset = rotation * new Vector3(0.0f, 0.0f, -cameraDistance);

        transform.position = playerObject.position + cameraOffset; // apply the rotation + offset
        transform.LookAt(playerObject.position); // orient the camera towards the player
    }
}
