using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using UnityEngine;
using Unity.Services.Lobbies.Models;
using UnityEditor.Search;
using UnityEngine.AI;
using System.Linq.Expressions;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.Assertions.Must;
using UnityEngine.Rendering;
using System.Runtime.InteropServices;
using NUnit.Framework;
using System.Net;

public class LobbyManager : MonoBehaviour
{

    public string lobbyName;
    public bool isLobbyServer = false;

    public Lobby lobby;

    public int lobbyCount;

    public AuthenticationManager authManager;
    public MatchmakerManager matchManager;

    public MultiplayManager mpManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public async void Start()
    {
        authManager = GetComponent<AuthenticationManager>();
        matchManager = GetComponent<MatchmakerManager>();
        mpManager = GetComponent<MultiplayManager>();

        await UnityServices.InitializeAsync();
        while (!authManager.isSignedIn)
        {
            await Task.Delay(1000);
        }

        QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync();
        lobbyCount = response.Results.Count;

        // TODO: remove
        await CreateLobby("test", "player", 8, false);
        await Task.Delay(10000);
        await QueryAvailableLobbies();
    }

    public async void JoinLobby(Lobby lobby)
    {
        try
        {
            await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id);
        }
        catch (Exception e)
        {
            Debug.Log("Could not join lobby due to Exception: " + e);
        }
        Debug.Log("Joined lobby: " + lobby.Data["serverIP"].Value);

        string serverIp = lobby.Data != null && lobby.Data.ContainsKey("serverIP") ? lobby.Data["serverIP"].Value : "Unknown";
        ushort serverPort = (ushort)(lobby.Data != null && lobby.Data.ContainsKey("serverPort") ? Convert.ToUInt16(lobby.Data["serverPort"].Value) : 0);

        mpManager.JoinToServer(serverIp, serverPort);
    }

    // Update is called once per frame
    void Update()
    {

    }

    // galleryId (fixed) != lobbyId (dynamic)
    public async Task CreateLobby(string galleryId, string playerId, int lobbyCapacity, bool isPrivate)
    {
        // TODO: maybe check if there already exists another lobby with the same galleryId

        Tuple<string, ushort> serverData = await matchManager.AllocateServer(playerId, galleryId);
        try
        {
            Debug.Log("attempting to create lobby");
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(galleryId, lobbyCapacity, new CreateLobbyOptions
            {
                IsPrivate = isPrivate,
                Data = new Dictionary<string, DataObject>{
            { "serverIP", new DataObject(DataObject.VisibilityOptions.Public, serverData.Item1) },
            { "serverPort", new DataObject(DataObject.VisibilityOptions.Public, serverData.Item2.ToString()) }}
            });
            Debug.Log("Lobby created: " + lobby.Id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("Error while creating lobby");
            Debug.Log(e.Message);
        }
    }

    public async Task<List<Lobby>> QueryAvailableLobbies()
    {
        List<Lobby> lobbies = new List<Lobby>();
        QueryLobbiesOptions queryOptions = new QueryLobbiesOptions { Count = 25 };

        try
        {
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryOptions);
            Debug.Log("Queried " + queryResponse.Results.Count + " Lobbies");

            if (queryResponse.Results.Count > 0)
            {
                lobbies = queryResponse.Results;
                foreach (Lobby lobby in lobbies)
                {
                    string serverIp = lobby.Data != null && lobby.Data.ContainsKey("serverIP") ? lobby.Data["serverIP"].Value : "Unknown";
                    string serverPort = lobby.Data != null && lobby.Data.ContainsKey("serverPort") ? lobby.Data["serverPort"].Value : "Unknown";
                    Debug.Log($"Lobby ID: {lobby.Id} attached to gallery {lobby.Name} with capacity {lobby.MaxPlayers} - Server IP: {serverIp} | Port: {serverPort}");
                }
            }
            else
            {
                Debug.Log("NO AVAILABLE LOBBIES FOUND");
            }
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError("Error querying Lobby:" + ex.Message);
        }
        return lobbies;
    }

    public async void LeaveLobby()
    {
        mpManager.DisconnectFromServer();
    }

    private void OnApplicationQuit()
    {
        LeaveLobby();
    }

    // should be called periodically while inside a gallery session to keep it alive
    public async void PingLobby()
    {
        while (true)
        {
            if (lobby == null) return;
            await LobbyService.Instance.SendHeartbeatPingAsync(lobby.Id);
            await Task.Delay(60 * 1000);
        }
    }
}

