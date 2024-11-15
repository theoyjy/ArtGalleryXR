using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float moveSpeed = 5.0f;

    void Update()
    {
        // get input of users
        float moveHorizontal = Input.GetAxis("Horizontal"); // control A and D
        float moveVertical = Input.GetAxis("Vertical"); // control W and S

        // calculate translate
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        
        // move it
        transform.Translate(movement * moveSpeed * Time.deltaTime, Space.World);
    }
}
