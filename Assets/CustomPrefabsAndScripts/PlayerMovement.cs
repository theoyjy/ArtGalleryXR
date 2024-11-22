using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    public float speed = 5f; // Movement speed
    public float mouseSensitivity = 100f; // Mouse sensitivity

    private float xRotation = 0f; // For vertical look control
    public Transform playerBody; // Reference to the player's body (parent transform)
    public Camera playerCamera; // Reference to the player's camera

    // NetworkVariable to sync the position
    private NetworkVariable<Vector3> networkedPosition = new NetworkVariable<Vector3>();

    private void Start()
    {
        if (!IsOwner)
        {
            // Disable the camera and script for non-local players
            playerCamera.gameObject.SetActive(false);
            enabled = false;
        }
        else
        {
            // Lock the cursor to the game window
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    private void Update()
    {
        if (IsOwner)
        {
            HandleMouseLook();
            HandleMovement();

            // Update the NetworkVariable with the current position
            networkedPosition.Value = transform.position;
        }
        else
        {
            // Smoothly interpolate position updates for non-owner clients
            transform.position = Vector3.Lerp(transform.position, networkedPosition.Value, Time.deltaTime * 10f);
        }
    }

    private void HandleMouseLook()
    {
        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Adjust the vertical rotation (clamp to avoid flipping)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Apply rotation to the camera
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotate the player body horizontally
        playerBody.Rotate(Vector3.up * mouseX);
    }

    private void HandleMovement()
    {
        // Get input for WASD movement
        float moveX = Input.GetAxis("Horizontal"); // A/D keys or Left/Right arrows
        float moveZ = Input.GetAxis("Vertical");   // W/S keys or Up/Down arrows

        // Calculate movement direction relative to the player body
        Vector3 move = playerBody.right * moveX + playerBody.forward * moveZ;

        // Apply movement
        transform.Translate(move * speed * Time.deltaTime, Space.World);
    }
}