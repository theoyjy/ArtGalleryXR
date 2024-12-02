using Unity.Netcode;
using UnityEngine;

public class ServerInitializer : MonoBehaviour
{
    void Start()
    {
        if (Application.isBatchMode)
        {
            NetworkManager.Singleton.StartServer();
        }
        else
        {
            NetworkManager.Singleton.StartClient();
        }
    }
}