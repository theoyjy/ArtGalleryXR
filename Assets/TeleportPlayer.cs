using UnityEngine;

public class TeleportPlayer : MonoBehaviour
{
    // Reference to the target location for teleportation
    public Transform teleportTarget;

    // Reference to the player (desktop camera or XR Rig)
    public GameObject player;

    // This function will be called when the button is clicked
    public void Teleport()
    {
        if (player != null && teleportTarget != null)
        {
            player.transform.position = teleportTarget.position;
            // Optionally, set player.transform.rotation = teleportTarget.rotation;
        }
        else
        {
            Debug.LogWarning("Player or Teleport Target not assigned!");
        }
    }
}
