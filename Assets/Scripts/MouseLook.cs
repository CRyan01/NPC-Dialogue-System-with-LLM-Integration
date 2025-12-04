using UnityEngine;

public class MouseLook : MonoBehaviour {
    // A reference to the player object
    public Transform playerObject;

    // A multiplier on the rotation
    public float mouseSensitivity = 150.0f;

    float xRotation;

    void Start() { 
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update() {
        // Get input values from the mouse
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Rotate the camera up or down
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90.0f, 90.0f);
        transform.localRotation = Quaternion.Euler(xRotation, 0.0f, 0.0f);

        // Rotate the player left or right
        playerObject.Rotate(Vector3.up * mouseX);
    }
}
