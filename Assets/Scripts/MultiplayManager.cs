using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Services.Core;
#if SERVER_BUILD
using Unity.Services.Multiplay;
#endif
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;

public class MultiplayManager : MonoBehaviour
{
    [SerializeField] private string ipAddress;
    [SerializeField] private ushort port;

#if SERVER_BUILD
    private IServerQueryHandler serverQueryHandler;
#endif

    private async void Start()
    {
#if SERVER_BUILD
            Application.targetFrameRate = 60;

            await UnityServices.InitializeAsync();

            ServerConfig serverConfig = MultiplayService.Instance.ServerConfig;
            serverQueryHandler = await MultiplayService.Instance.StartServerQueryHandlerAsync(10, "MyServer", "MyGameType", "0", "TestMap");

            if (serverConfig.AllocationId != string.Empty)
            {
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData("0.0.0.0", serverConfig.Port, "0.0.0.0");
                NetworkManager.Singleton.StartServer();

                await MultiplayService.Instance.ReadyServerForPlayersAsync();
            }
#endif
        JoinToServer();
    }

    private async void Update()
    {
#if SERVER_BUILD
        
            if (serverQueryHandler != null)
            {
                serverQueryHandler.CurrentPlayers = (ushort)NetworkManager.Singleton.ConnectedClientsIds.Count;
                serverQueryHandler.UpdateServerCheck();
                await Task.Delay(100);
            }
#endif
    }

    public void JoinToServer()
    {
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(ipAddress, port);

        NetworkManager.Singleton.StartClient();
    }

}
