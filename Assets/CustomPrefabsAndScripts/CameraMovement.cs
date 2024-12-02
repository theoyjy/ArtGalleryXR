using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float moveSpeed = 5.0f;
    public float rotationSpeed = 3.0f;

    private float yaw = 0.0f;
    private float pitch = 0.0f;

    void Update()
    {
        // Keyboard input to control camera movement
        float moveHorizontal = Input.GetAxis("Horizontal"); // A and D keys for left/right movement
        float moveVertical = Input.GetAxis("Vertical"); // W and S keys for forward/backward movement

        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        transform.Translate(movement * moveSpeed * Time.deltaTime, Space.World);

        // Mouse drag to change the camera view
        if (Input.GetMouseButton(0)) // Left mouse button is pressed
        {
            yaw += Input.GetAxis("Mouse X") * rotationSpeed;
            pitch -= Input.GetAxis("Mouse Y") * rotationSpeed;

            // Clamp the pitch angle to prevent flipping
            pitch = Mathf.Clamp(pitch, -90f, 90f);

            // Set the camera's rotation
            transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        }
    }
}