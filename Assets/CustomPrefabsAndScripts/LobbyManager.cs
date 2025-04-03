using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using UnityEngine;
using Unity.Services.Lobbies.Models;
//using UnityEditor.Search;
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
        // await CreateLobby("test", "player", 8, false, "");
        // await Task.Delay(1000);
        // await CreateLobby("test2private", "player2", 12, true, "898804djk");
        // await Task.Delay(1000);
        // await CreateLobby("test3", "player3", 12, false, "");
        // await Task.Delay(1000);
        // await CreateLobby("test4", "player4", 12, false, "");
        // await Task.Delay(1000);
        // await CreateLobby("test5", "player5", 12, false, "");
        // await Task.Delay(1000);
         List<Lobby> lobbyResults = await QueryAvailableLobbies();
        // lobby = lobbyResults[0];
        // await JoinLobby(lobby);
    }

    public async Task JoinLobby(Lobby lobby)
    {
        try
        {
            await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id);
        }
        catch (Exception e)
        {
            Debug.Log("Could not join lobby due to Exception: " + e);
        }

        string serverIp = lobby.Data != null && lobby.Data.ContainsKey("serverIP") ? lobby.Data["serverIP"].Value : "Unknown";
        ushort serverPort = (ushort)(lobby.Data != null && lobby.Data.ContainsKey("serverPort") ? Convert.ToUInt16(lobby.Data["serverPort"].Value) : 0);
        mpManager.JoinToServer(serverIp, serverPort);
        Debug.Log($"Joined lobby with server: {serverIp}:{serverPort}");
    }

    // Update is called once per frame
    void Update()
    {

    }

    // galleryId (fixed) != lobbyId (dynamic)
    public async Task<string> CreateLobby(string galleryId, string playerId, int lobbyCapacity, bool isPrivate, string password)
    {
        string lobbyId =  "";
        // TODO: maybe check if there already exists another lobby with the same galleryId

        Tuple<string, ushort> serverData = await matchManager.AllocateServer(playerId, galleryId);
        try
        {
            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>{
                    { "serverIP", new DataObject(DataObject.VisibilityOptions.Public, serverData.Item1) },
                    { "serverPort", new DataObject(DataObject.VisibilityOptions.Public, serverData.Item2.ToString()) }
                }
            };

            if (isPrivate)
            {
                lobbyOptions.Password = password;
            }

            Debug.Log("attempting to create lobby");
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(galleryId, lobbyCapacity, lobbyOptions);
            lobbyId = lobby.Id;
            Debug.Log("Lobby created: " + lobbyId);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("Error while creating lobby: " + e);
        }
        return lobby.Id;
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
                    Debug.Log($"{(lobby.HasPassword ? "(PRIVATE)" : "(PUBLIC)")} Lobby ID: {lobby.Id} created by: {lobby.HostId} attached to: Gallery '{lobby.Name}' with capacity: {lobby.MaxPlayers}, Member count: {lobby.Players.Count} - Server IP: {serverIp} | Port: {serverPort}");
                }
            }
            else
            {
                Debug.Log("NO AVAILABLE LOBBIES FOUND");
            }
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError("Error querying Lobby:" + ex);
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

