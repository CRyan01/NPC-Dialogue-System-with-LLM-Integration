using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerMovement : MonoBehaviour {
    private CharacterController characterController; // A reference to the players character controller component

    public Transform cameraTransform; // A reference to the camera transform

    public float moveSpeed = 6.0f; // how fast the player moves on the ground
    public float jumpHeight = 1.5f; // how high the player can jump
    public float gravityStrength = -9.50f; // how much gravity the player is under

    private float turnSmoothTime = 0.1f; // the time it takes to reach the target facing direction
    private float turnSmoothVelocity; // interal velocity for SmoothDampAngle

    private float verticalVelocity; // the players current vertical velocity

    void Start() {
        // Get and store a reference to the players character controller component
        characterController = GetComponent<CharacterController>();
    }

    private void Update() {
        // Handle walking, turning, jumping, and gravity
        HandleMovement();
    }

    private void HandleMovement() {
        // Capture movement input from the keyboard (WASD or arrow keys)
        float inputX = Input.GetAxis("Horizontal");
        float inputZ = Input.GetAxis("Vertical");
        
        // Store movement input in the X,Z plane
        Vector3 inputVector = new Vector3(inputX, 0.0f, inputZ);

        // Determine if there was any movement input this frame
        bool hasMovementInput = inputVector.sqrMagnitude > 0.001f;

        // Determine if the player is on the ground at the start of the frame
        bool isGrounded = characterController.isGrounded;

        // If the player is on the ground and still moving downwards, reset vertical velocity
        if (isGrounded && verticalVelocity < 0.0f) {
            verticalVelocity = -2.0f;
        }

        // If the player presses space while on the ground, start a jump
        if (isGrounded && Input.GetKeyDown(KeyCode.Space)) {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2.0f * gravityStrength);
        }

        // Add manual gravity so the player falls when not on the ground
        verticalVelocity += gravityStrength * Time.deltaTime;

        // Build a vector to store move direction realtive to the camera
        Vector3 moveDirection = Vector3.zero;

        // Check if there was movement input this frame
        if (hasMovementInput) {
            // Convert input direction to an angle (in degrees) and offset by camera rotation
            float inputAngle = Mathf.Atan2(inputVector.x, inputVector.z) * Mathf.Rad2Deg;
            float targetAngle = inputAngle + cameraTransform.eulerAngles.y;

            // Get the current y facing angle (Y-axis)
            float currentYAngle = transform.eulerAngles.y;

            // Smoothly rotate towards the target angle using angle-aware damping
            float smoothedYAngle = Mathf.SmoothDampAngle(
                currentYAngle,
                targetAngle,
                ref turnSmoothVelocity,
                turnSmoothTime
                );
            transform.rotation = Quaternion.Euler(0.0f, smoothedYAngle, 0.0f);

            // Move forward in the target direction (not the smoothed one)
            moveDirection = Quaternion.Euler(0.0f, targetAngle, 0.0f) * Vector3.forward;
        }

        // Build the final movement vector
        Vector3 finalMovement = moveDirection * moveSpeed;
        finalMovement.y = verticalVelocity;

        characterController.Move(finalMovement * Time.deltaTime); // apply the movement
    }
}
