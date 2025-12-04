using UnityEngine;

public class CameraFollow : MonoBehaviour {
    // The target object to follow
    public Transform cameraTarget;

    // The cameras offset from the target object
    public Vector3 cameraOffset = new Vector3(0.0f, 6.0f, -8.0f);

    // The speed at which the camera moves to its new position
    public float speed = 10.0f;

    private void LateUpdate() {
        // Check if the camera has a target
        if (cameraTarget == null) {
            return; // return if null
        }

        // The cameras desired position
        Vector3 newPos = cameraTarget.position + cameraOffset;

        // Interpolate from the cameras current position to its new position
        transform.position = Vector3.Lerp(transform.position, newPos, speed * Time.deltaTime);

        // Orient the camera towards its target
        //transform.LookAt(cameraTarget);
    }
}
