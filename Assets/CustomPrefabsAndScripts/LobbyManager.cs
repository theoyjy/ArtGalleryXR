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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public async void Start()
    {
        authManager = GetComponent<AuthenticationManager>();
        matchManager = GetComponent<MatchmakerManager>();

        if (UnityServices.State.Equals(ServicesInitializationState.Initialized))
        {
        await UnityServices.InitializeAsync();
        }

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

    public async Task JoinLobby(Lobby lobby, string password, bool isGuest)
    {
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
                return;
            }

        }

        string serverIp = lobby.Data != null && lobby.Data.ContainsKey("serverIP") ? lobby.Data["serverIP"].Value : "Unknown";
        ushort serverPort = (ushort)(lobby.Data != null && lobby.Data.ContainsKey("serverPort") ? Convert.ToUInt16(lobby.Data["serverPort"].Value) : 0);

        Debug.Log($"Joined lobby with server: {serverIp}:{serverPort}");

        // Save a reference to the selected lobby so that we can access it inside the gallery
        SharedDataManager.CurrentLobby = lobby;
        SceneManager.LoadScene("Gallery");

        //var asyncOp = SceneManager.LoadSceneAsync("Gallery", LoadSceneMode.Single);

        //// Wait until the scene is fully loaded
        //while (!asyncOp.isDone)
        //{
        //    await Task.Yield(); // or use a coroutine if you're not using async/await
        //}

        //// Now it's safe to get and activate the scene
        //Scene galleryScene = SceneManager.GetSceneByName("Gallery");

        //if (galleryScene.IsValid() && galleryScene.isLoaded)
        //{
        //    SceneManager.SetActiveScene(galleryScene);
        //}
        //else
        //{
        //    Debug.LogError("Failed to load or find the 'Gallery' scene.");
        //}
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