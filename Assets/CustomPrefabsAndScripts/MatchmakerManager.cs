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

public class MatchmakerManager : MonoBehaviour
{
    public bool isAllocated = false;
    public string allocatedIpAddress;
    public ushort allocatedPort;

    bool isDeallocating = false;
    public string backfillIpAddress;
    public ushort backfillPort;
    public bool isServerAvailable = false;
    public MultiplayManager mpManager;
    private string backfillTicketId;
    private bool isBackfilling = false;

    public async void Start()
    {
        mpManager = GetComponent<MultiplayManager>();

        if (Application.platform != RuntimePlatform.LinuxServer)
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            await QueryAvailableServers();
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

    public async void AllocateServer()
    {
        var players = new List<Unity.Services.Matchmaker.Models.Player> { new("Player1", new Dictionary<string, object>()) };
        var options = new CreateTicketOptions("Gallery-A", new Dictionary<string, object>());
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
    }

    async Task QueryAvailableServers()
    {
        QueryLobbiesOptions queryOptions = new QueryLobbiesOptions { Count = 25 };

        try
        {
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryOptions);
            Debug.Log("Queried " + queryResponse.Results.Count + " Lobbies");

            if (queryResponse.Results.Count > 0)
            {
                foreach (Lobby lobby in queryResponse.Results)
                {
                    string lobbyId = lobby.Id;
                    string serverIp = lobby.Data != null && lobby.Data.ContainsKey("ip") ? lobby.Data["ip"].Value : "Unknown";
                    string serverPort = lobby.Data != null && lobby.Data.ContainsKey("port") ? lobby.Data["port"].Value : "Unknown";

                    Debug.Log($"Lobby ID: {lobbyId} - Server IP: {serverIp} | Port: {serverPort}");
                    isServerAvailable = true;
                }
            }
            else
            {
                Debug.Log("NO AVAILABLE SERVER CURRENTLY");
            }
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError("Error querying Lobby:" + ex.Message);
        }
    }
}