using UnityEngine;
using Unity.Netcode;

public class PlayerCameraSetup : NetworkBehaviour
{
    public Camera playerCamera;

    private void Start()
    {
        if (IsOwner)
        {
            // Activate the camera for the local player
            playerCamera.gameObject.SetActive(true);
        }
        else
        {
            // Deactivate the camera for non-local players
            playerCamera.gameObject.SetActive(false);
        }
    }
}