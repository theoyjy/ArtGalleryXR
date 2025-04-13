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
using Unity.Collections;

public class MultiplayManager : MonoBehaviour
{
    public string ipAddress;
    public ushort port;
    public bool hasServerData = false;
    private float deallocationTimer = 0f;
    private bool isWaitingForDeallocation = false;
    public GalleryManager galleryManager;
    public Lobby lobby;

#if SERVER_BUILD
    private IServerQueryHandler serverQueryHandler;
    private ulong hostClientId = ulong.MaxValue;
    private bool isHostConnected = true;
#endif

    private async void Start()
    {
#if SERVER_BUILD
        Application.targetFrameRate = 60;

        await UnityServices.InitializeAsync();

        ServerConfig serverConfig = MultiplayService.Instance.ServerConfig;
        serverQueryHandler = await MultiplayService.Instance.StartServerQueryHandlerAsync(40, "gallery", "standard", "0", "gallery");

        if (serverConfig.AllocationId != string.Empty)
        {
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData("0.0.0.0", serverConfig.Port, "0.0.0.0");
            NetworkManager.Singleton.StartServer();

            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnect;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;

            await MultiplayService.Instance.ReadyServerForPlayersAsync();
        }
#endif

#if !SERVER_BUILD
        galleryManager = GameObject.Find("GalleryManager").GetComponent<GalleryManager>();

        NetworkManager.Singleton.OnClientDisconnectCallback += HandleServerDisconnect;

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

#if SERVER_BUILD
    private async void Update()
    {
        if (serverQueryHandler != null)
        {
            serverQueryHandler.CurrentPlayers = (ushort)NetworkManager.Singleton.ConnectedClientsIds.Count;
            serverQueryHandler.UpdateServerCheck();
            await Task.Delay(100);
        }

        int playerCount = NetworkManager.Singleton.ConnectedClientsList.Count;

        if (!isHostConnected)
        {
            if (!isWaitingForDeallocation)
            {
                isWaitingForDeallocation = true;
                deallocationTimer = Time.time + 10f;
            }

            if (Time.time >= deallocationTimer)
            {
                Application.Quit();
                Debug.Log("Deallocating Server because the host left.");
            }
        }
        else if (playerCount == 0)
        {
            if (!isWaitingForDeallocation)
            {
                isWaitingForDeallocation = true;
                deallocationTimer = Time.time + 60f;
            }

            if (Time.time >= deallocationTimer)
            {
                Application.Quit();
                Debug.Log("Deallocating Server because no players are connected.");
            }
        }
        else
        {
            isWaitingForDeallocation = false;
        }
    }
#endif

    public void JoinToServer(string serverIp, ushort serverPort)
    {
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        transport.SetConnectionData(serverIp, serverPort);
        NetworkManager.Singleton.StartClient();
        Debug.Log($"Joined server: {serverIp}:{serverPort}");
    }

    public void DisconnectFromServer()
    {
        if (NetworkManager.Singleton.IsConnectedClient)
        {
            NetworkManager.Singleton.Shutdown();
            Debug.Log("Disconnected from server.");

            // Manually destroy the NetworkManager object (which is DontDestroyOnLoad)
            Destroy(NetworkManager.Singleton.gameObject);

            // If CanvasComponent is another persistent object, destroy it too
            GameObject canvas = GameObject.FindGameObjectWithTag("CanvasComponent");
            if (canvas != null)
            {
                Destroy(canvas.gameObject);
            }
        }
    }

#if !SERVER_BUILD
    private void HandleServerDisconnect(ulong clientId)
    {
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            Debug.LogWarning("Disconnected from server!");
            galleryManager.LeaveGallery();
        }
    }
#endif

#if SERVER_BUILD
    private void HandleClientConnect(ulong clientId)
    {
        Debug.Log($"Client connected: {clientId}");
        if (hostClientId == ulong.MaxValue)
        {
            hostClientId = clientId;
            Debug.Log($"Host assigned: ClientId {hostClientId}");
        }
    }

    private void HandleClientDisconnect(ulong clientId)
    {
        Debug.Log($"Client disconnected: {clientId}");

        if (clientId == hostClientId)
        {
            Debug.Log("Host has disconnected. Starting deallocation process.");
            isHostConnected = false;
        }
    }
#endif
}