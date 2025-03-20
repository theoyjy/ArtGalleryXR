using UnityEngine;
using Unity.Collections;
using Unity.Services.Multiplay;
using Unity.Services.Core;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using NUnit.Framework;
using Unity.VisualScripting;
using Unity.Netcode;
using System.Net.Http;
using System.Collections;
using UnityEngine.UIElements;
using TMPro;

public class MatchmakerManager : MonoBehaviour
{
    public bool isAllocated = false;
    public string allocatedIpAddress;
    public ushort allocatedPort;

    // [SerializeField] private TMP_InputField lobbyNameField;
    // [SerializeField] private Toggle isPrivateToggle;

    public bool isPrivate = false;
    public String lobbyName;

    bool isDeallocating = false;
    public string backfillIpAddress;
    public ushort backfillPort;
    public bool isServerAvailable = false;
    public MultiplayManager mpManager;
    public AuthenticationManager authManager;
    private string backfillTicketId;
    private bool isBackfilling = false;

    public LobbyManager lbyManager;

    public async void Start()
    {
        mpManager = GetComponent<MultiplayManager>();
        lbyManager = GetComponent<LobbyManager>();
        authManager = GetComponent<AuthenticationManager>();
        await UnityServices.InitializeAsync();
        
        while (!authManager.isSignedIn) {
            await Task.Delay(1000);
        }
        if (Application.platform != RuntimePlatform.LinuxServer)
        {
            // await QueryAvailableServers();
            if (isServerAvailable)
            BackfillServer();
        }
    }

    private float deallocationTimer = 0f;
    private bool isWaitingForDeallocation = false;

    async void Update()
    {
        int playerCount = NetworkManager.Singleton.ConnectedClientsList.Count;

        #if SERVER_BUILD

        if (playerCount == 0)
        {
            if (!isWaitingForDeallocation)
            {
                isWaitingForDeallocation = true;
                deallocationTimer = Time.time + 60f;
            }

            if (Time.time >= deallocationTimer)
            {
                isDeallocating = true;
                Application.Quit();
                Debug.Log("Deallocating Server");
            }
        }
        else
        {
            isWaitingForDeallocation = false;
        }

        if (backfillTicketId != null && NetworkManager.Singleton.ConnectedClientsList.Count < 4)
            {
                Debug.Log("Approving backfill ticket with ID: " + backfillTicketId);
                BackfillTicket backfillTicket = await MatchmakerService.Instance.ApproveBackfillTicketAsync(backfillTicketId);
                Debug.Log("Backfill ticket approved: " + backfillTicket.Id);
                backfillTicketId = backfillTicket.Id;
            }

        #endif

        if (Application.platform != RuntimePlatform.LinuxServer)
        {
            if (isAllocated && playerCount < 2 && !isBackfilling)
            {
                BackfillServer();
            }
            else if (isBackfilling && playerCount >= 2)
            {
                CancelBackfill();
            }
        }

        // await QueryAvailableServers();
    }

    public async void BackfillServer()
    {
        try {
        Debug.Log($"Attempting backfill with IP: {backfillIpAddress}, Port: {backfillPort}");
        var options = new CreateBackfillTicketOptions("Gallery-A", backfillIpAddress + ":" + backfillPort, new Dictionary<string, object>());
        Debug.Log("Creating backfill ticket with options: " + options);
        backfillTicketId = await MatchmakerService.Instance.CreateBackfillTicketAsync(options);
        isBackfilling = true;
        Debug.Log("Backfill ticket created: " + backfillTicketId);
        }
        catch (HttpRequestException ex)
        {
            Debug.LogError($"Backfill request failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Unexpected error in BackfillServer: {ex}");
        }
    }

    public async void CancelBackfill()
    {
        if (!string.IsNullOrEmpty(backfillTicketId))
        {
            await MatchmakerService.Instance.DeleteBackfillTicketAsync(backfillTicketId);
            Debug.Log("Backfill ticket cancelled");
        }
        isBackfilling = false;
    }

    public async Task<Tuple<string, ushort>> AllocateServer(string playerId, string galleryId)
    {
        var players = new List<Unity.Services.Matchmaker.Models.Player> { new(playerId, new Dictionary<string, object>()) };
        var options = new CreateTicketOptions(galleryId, new Dictionary<string, object>());
        var ticketResponse = await MatchmakerService.Instance.CreateTicketAsync(players, options);

        Debug.Log(ticketResponse.Id);

        MultiplayAssignment assignment = null;

        do
        {
            await Task.Delay(TimeSpan.FromSeconds(2f));
            var ticketStatus = await MatchmakerService.Instance.GetTicketAsync(ticketResponse.Id);
            if (ticketStatus == null)
            {
                continue;
            }

            if (ticketStatus.Type == typeof(MultiplayAssignment))
            {
                assignment = ticketStatus.Value as MultiplayAssignment;
            }

            switch (assignment?.Status)
            {
                case MultiplayAssignment.StatusOptions.Found:
                    Debug.Log("Match found! Server IP: " + assignment.Ip + " | Port: " + assignment.Port);
                    allocatedIpAddress = assignment.Ip;
                    allocatedPort = (ushort)assignment.Port;
                    isAllocated = true;
                    break;
                case MultiplayAssignment.StatusOptions.InProgress:
                    break;
                case MultiplayAssignment.StatusOptions.Failed:
                    isAllocated = false;
                    Debug.LogError("Failed to get ticket status. Error: " + assignment.Message);
                    break;
                case MultiplayAssignment.StatusOptions.Timeout:
                    isAllocated = false;
                    Debug.LogError("Failed to get ticket status. Ticket timed out.");
                    break;
                default:
                    throw new InvalidOperationException();
            }
        } while (!isAllocated);

        mpManager.ipAddress = allocatedIpAddress;
        mpManager.port = (ushort)allocatedPort;
        mpManager.hasServerData = isAllocated;

        return new Tuple<string, ushort>(allocatedIpAddress, allocatedPort);

        // await lbyManager.CreateLobby(lobbyName, isPrivate, allocatedIpAddress, allocatedPort);

    }

    async Task<List<Lobby>> QueryAvailableLobbies()
    {
        List<Lobby> lobbies = new List<Lobby>();
        QueryLobbiesOptions queryOptions = new QueryLobbiesOptions { Count = 25 };

        try
        {
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryOptions);
            Debug.Log("Queried " + queryResponse.Results.Count + " Lobbies");

            if (queryResponse.Results.Count > 0)
            {
                lobbies =  queryResponse.Results;
                foreach (Lobby lobby in lobbies)
                {
                    string lobbyId = lobby.Id;
                    string galleryId = lobby.Name;
                    string serverIp = lobby.Data != null && lobby.Data.ContainsKey("serverIP") ? lobby.Data["serverIP"].Value : "Unknown";
                    string serverPort = lobby.Data != null && lobby.Data.ContainsKey("serverPort") ? lobby.Data["serverPort"].Value : "Unknown";

                    Debug.Log($"Lobby ID: {lobbyId} attached to gallery {galleryId} - Server IP: {serverIp} | Port: {serverPort}");
                    isServerAvailable = true;
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
}