using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using UnityEngine;
using Unity.Services.Lobbies.Models;
using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class LobbyManager : MonoBehaviour
{
    public Lobby lobby;
    public AuthenticationManager authManager;
    public MatchmakerManager matchManager;

    public enum JoinStatus
    {
        SUCCESS,
        GALLERY_FULL,
        WRONG_PASSWORD,
        GALLERY_OFFLINE
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public async void Start()
    {
        authManager = GetComponent<AuthenticationManager>();
        matchManager = GetComponent<MatchmakerManager>();

        await UnityServices.InitializeAsync();
        while (!authManager.isSignedIn)
        {
            await Task.Delay(1000);
        }

        // TODO: remove
        // await CreateLobby("test", "player", 8, false, "");
        // await Task.Delay(1000);
        //await CreateLobby("test2private", "player2", 12, true, "898804djk");
        // await Task.Delay(1000);
        // await CreateLobby("test3", "player3", 12, false, "");
        // await Task.Delay(1000);
        // await CreateLobby("test4", "player4", 12, false, "");
        // await Task.Delay(1000);
        // await CreateLobby("test5", "player5", 12, false, "");
        //await Task.Delay(1000);
        // List<Lobby> lobbyResults = await QueryAvailableLobbies();
        // lobby = lobbyResults[0];
        // await JoinLobby(lobby, "", false);
    }

    public async Task<JoinStatus> JoinLobby(Lobby lobby, string password, bool isGuest)
    {
        var status = JoinStatus.SUCCESS;

        var joinOptions = new JoinLobbyByIdOptions { };

        if (isGuest)
        {
            if (lobby.HasPassword)
            {
                joinOptions.Password = password;
            }

            try
            {
                await LobbyService.Instance.JoinLobbyByIdAsync(lobby.Id, joinOptions);
            }
            catch (LobbyServiceException ex)
            {
                Debug.LogError("Failed to join lobby: " + ex);
                switch (ex.Reason)
                {
                    case LobbyExceptionReason.LobbyFull:
                        return JoinStatus.GALLERY_FULL;
                    case LobbyExceptionReason.LobbyNotFound:
                        return JoinStatus.GALLERY_OFFLINE;
                    case LobbyExceptionReason.IncorrectPassword:
                        return JoinStatus.WRONG_PASSWORD;
                }
            }


        }

        string serverIp = lobby.Data != null && lobby.Data.ContainsKey("serverIP") ? lobby.Data["serverIP"].Value : "Unknown";
        ushort serverPort = (ushort)(lobby.Data != null && lobby.Data.ContainsKey("serverPort") ? Convert.ToUInt16(lobby.Data["serverPort"].Value) : 0);

        Debug.Log($"Joined lobby with server: {serverIp}:{serverPort}");

        // Save a reference to the selected lobby so that we can access it inside the gallery
        SharedDataManager.CurrentLobby = lobby;
        SceneManager.LoadScene("Gallery");

        return status;
    }

    // Update is called once per frame
    void Update()
    {

    }

    // galleryId (fixed) != lobbyId (dynamic)
    public async Task<string> CreateLobby(string galleryId, string playerId, int lobbyCapacity, bool isPrivate, string password)
    {
        // TODO: maybe check if there already exists another lobby with the same galleryId
        string lobbyID = "";
        Tuple<string, ushort> serverData = await matchManager.AllocateServer(playerId, galleryId);
        try
        {
            CreateLobbyOptions lobbyOptions = new CreateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>{
                    { "serverIP", new DataObject(DataObject.VisibilityOptions.Public, serverData.Item1) },
                    { "serverPort", new DataObject(DataObject.VisibilityOptions.Public, serverData.Item2.ToString()) },
                    { "hostPlayerId", new DataObject(DataObject.VisibilityOptions.Public, playerId.ToString()) },
                }
            };

            if (isPrivate)
            {
                lobbyOptions.Password = password;
            }

            Debug.Log("attempting to create lobby");
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(galleryId, lobbyCapacity, lobbyOptions);
            Debug.Log("Lobby created: " + lobby.Id);
            lobbyID = lobby.Id;

            // await JoinLobby(lobby, false);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("Error while creating lobby: " + e);
        }
        return lobbyID;
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
                    string hostPlayerId = lobby.Data != null && lobby.Data.ContainsKey("hostPlayerId") ? lobby.Data["hostPlayerId"].Value : "Unknown";
                    Debug.Log($"{(lobby.HasPassword ? "(PRIVATE)" : "(PUBLIC)")} Lobby ID: {lobby.Id} created by: {hostPlayerId} attached to: Gallery '{lobby.Name}' with capacity: {lobby.MaxPlayers}, Member count: {lobby.Players.Count} - Server IP: {serverIp} | Port: {serverPort}");
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
}