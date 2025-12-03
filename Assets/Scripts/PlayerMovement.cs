using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    // The players character controller component
    private CharacterController characterController;

    // How fast the player moves
    public float speed = 6.0f;
    // How high the player can jump
    public float jumpHeight = 1.5f;
    // How much gravity the player is under
    public float gravity = -9.50f;

    // The players current vertical velocity
    private float verticalVelocity;

    void Start() {
        // Get a reference to the players character controller component
        characterController = GetComponent<CharacterController>();
    }

    private void Update() {
        // Capture movement input from the keyboard
        float inputX = Input.GetAxis("Horizontal");
        float inputZ = Input.GetAxis("Vertical");
        
        // Read the grounded state at the start of each frame
        bool isGrounded = characterController.isGrounded;

        // Handle jumping
        if (isGrounded && verticalVelocity < 0.0f) {
            verticalVelocity = -2.0f; // add a grounding effect
        }

        // Check for input
        if (isGrounded && Input.GetKeyDown(KeyCode.Space)) {
            // Calulate the vertical movement
            Debug.Log("Space pressed!", this);
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2.0f * gravity);
        }

        // Add manual gravity
        verticalVelocity += gravity * Time.deltaTime;

        // Build the movement vector
        Vector3 movement = (transform.right * inputX + transform.forward * inputZ) * speed;
        movement.y = verticalVelocity; // apply vertical velocity (if any)

        // Apply the motion
        characterController.Move(movement * Time.deltaTime);
    }
}
