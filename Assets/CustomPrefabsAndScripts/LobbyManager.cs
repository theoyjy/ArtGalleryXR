using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using UnityEngine;
using Unity.Services.Lobbies.Models;
using UnityEditor.Search;
using UnityEngine.AI;

public class LobbyManager : MonoBehaviour
{

    public string lobbyName;
    public bool isLobbyServer = false;
    public int lobbyCapacity;

    public Lobby lobby;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public async void Start()
    {
        await UnityServices.InitializeAsync();
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync();
        Debug.Log(response.Results.Count + " lobbies found");
        isLobbyServer = response.Results.Count == 0;

        if (!isLobbyServer) {
            lobby = response.Results[0];
        }

        if (isLobbyServer)
        {
            await CreateLobby();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isLobbyServer) LobbyHeartbeat();
    }

    public async Task CreateLobby()
    {
        try
        {
            Debug.Log("attempting to create lobby");
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, lobbyCapacity);
            Debug.Log("Lobby created: " + lobby.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("Error");
            Debug.Log(e.Message);
        }
    }

    private async void LobbyHeartbeat()
    {
        while (true)
        {
            if (lobby == null) return;
            await LobbyService.Instance.SendHeartbeatPingAsync(lobby.Id);
            await Task.Delay(60 * 1000);
        }
    }
}

