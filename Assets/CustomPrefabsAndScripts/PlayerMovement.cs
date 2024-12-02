using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : NetworkBehaviour
{
    public float speed = 5f;

    private NetworkVariable<Vector3> networkedPosition = new NetworkVariable<Vector3>();

    private void Update()
    {
        if (IsOwner)
        {
            HandleMovement();
            networkedPosition.Value = transform.position; // Update NetworkVariable
        }
        else
        {
            // Apply synchronized position to non-owner clients
            transform.position = Vector3.Lerp(transform.position, networkedPosition.Value, Time.deltaTime * 10f);
        }
    }

    private void HandleMovement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = new Vector3(moveX, 0f, moveZ) * speed * Time.deltaTime;
        transform.Translate(move, Space.World);
    }
}
