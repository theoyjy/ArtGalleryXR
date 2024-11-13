using UnityEngine;

public class KeyboardControl : MonoBehaviour
{
    public float moveSpeed = 5f; // Speed of movement

    void Update()
    {
        // Get input for horizontal and vertical axes
        float horizontal = Input.GetAxis("Horizontal"); // A/D or Left/Right Arrow keys
        float vertical = Input.GetAxis("Vertical");     // W/S or Up/Down Arrow keys

        // Move the object based on input and moveSpeed
        Vector3 movement = new Vector3(horizontal, 0, vertical) * moveSpeed * Time.deltaTime;
        transform.Translate(movement, Space.World);
    }
}
