using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

using Unity.Services.Core;
#if SERVER_BUILD
using Unity.Services.Multiplay;
#endif
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;
using Unity.Services.Lobbies.Models;

public class MultiplayManager : MonoBehaviour
{
    public string ipAddress;
    public ushort port;
    public bool hasServerData = false;

    public GalleryManager galleryManager;

    public Lobby lobby;

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

#if !SERVER_BUILD
        galleryManager = GameObject.Find("GalleryManager").GetComponent<GalleryManager>();

        while (!hasServerData)
        {
            if (galleryManager.currentLobby != null)
            {
                lobby = galleryManager.currentLobby;
                hasServerData = true;
                break;
            }
        }

        ipAddress = lobby.Data["serverIP"].Value;
        port = Convert.ToUInt16(lobby.Data["serverPort"].Value);
        try
        {
            JoinToServer(ipAddress, port);
        }
        catch (Exception e)
        {
            Debug.Log("Could not join gallery: " + e);
            galleryManager.GoToLobbies();
        }
#endif
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



    public void JoinToServer(string serverIp, ushort serverPort)
    {
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(serverIp, serverPort);
        NetworkManager.Singleton.StartClient();
        Debug.Log($"Joined server: {serverIp}:{serverPort}");
    }

    public void DisconnectFromServer()
    {
        if (Application.platform != RuntimePlatform.LinuxServer)
        {
            if (NetworkManager.Singleton.IsConnectedClient)
            {
                NetworkManager.Singleton.Shutdown(true);
                NetworkManager.Singleton.DisconnectClient(NetworkManager.Singleton.LocalClientId);
            }
        }
    }

}
