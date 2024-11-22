using Unity.Netcode;
using UnityEngine;
using Unity.Netcode.Transports.UTP;

public class MultiplayerManager : MonoBehaviour
{
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(20, 20, 500, 500));

        if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
        {
            if (GUILayout.Button("Start Host"))
            {
                NetworkManager.Singleton.StartHost();
            }
            if (GUILayout.Button("Start Client"))
            {
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData("192.168.156.155", 7777);
                NetworkManager.Singleton.StartClient();
            }
            if (GUILayout.Button("Start Server"))
            {
                NetworkManager.Singleton.StartServer();
            }
        }
        else
        {
            if (GUILayout.Button("Stop"))
            {
                NetworkManager.Singleton.Shutdown();
            }
        }

        GUILayout.EndArea();
    }
}
