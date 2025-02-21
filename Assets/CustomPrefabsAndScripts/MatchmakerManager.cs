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


public class MatchmakerManager : MonoBehaviour
{
    public bool isAllocated = false;
    public string allocatedIpAddress;
    public ushort allocatedPort;

    bool isDeallocating = false;

    public string backfillIpAddress;
    public ushort backfillPort;
    public bool isServerAvaliable = false;

    public MultiplayManager mpManager;

    public async void Start()
    {
        mpManager = GetComponent<MultiplayManager>();

        if (Application.platform != RuntimePlatform.LinuxServer)
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();

        }
        await QueryAvailableServers();
        if (!isServerAvaliable)
            AllocateServer();
    }

    private float deallocationTimer = 0f;
    private bool isWaitingForDeallocation = false;

    void Update()
    {
        if (NetworkManager.Singleton.ConnectedClientsList.Count == 0)
        {
            if (!isWaitingForDeallocation) // Start the timer only once
            {
                isWaitingForDeallocation = true;
                deallocationTimer = Time.time + 60f; // Set timer for 60 seconds
            }

            if (Time.time >= deallocationTimer) // Check if 60s have passed
            {
                isDeallocating = true;
                Application.Quit();
                Debug.Log("Deallocating Server");
            }
        }
        else
        {
            isWaitingForDeallocation = false; // Reset if a player joins
        }
    }


    public async void BackfillServer()
    {
        var options = new CreateBackfillTicketOptions("Gallery-A", backfillIpAddress + ":" + backfillPort, new Dictionary<string, object>());
        string ticketId = await MatchmakerService.Instance.CreateBackfillTicketAsync(options);
    }

    public async void waitForBackfillTickets()
    {
        // do {

        // }
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
            //Rate limit delay
            await Task.Delay(TimeSpan.FromSeconds(2f));

            // Poll ticket
            var ticketStatus = await MatchmakerService.Instance.GetTicketAsync(ticketResponse.Id);
            if (ticketStatus == null)
            {
                continue;
            }

            //Convert to platform assignment data (IOneOf conversion)
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
                    //...
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

        QueryLobbiesOptions queryOptions = new QueryLobbiesOptions
        {
            Count = 25
        };

        try
        {
            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryOptions);
            Debug.Log("Queried " + queryResponse.Results.Count + " Lobbies");
            Debug.Log("Query Response: " + queryResponse.Results.Count);

            if (queryResponse.Results.Count > 0)
            {
                foreach (Lobby lobby in queryResponse.Results)
                {
                    //  Default extraction of Lobby ID and server information stored in Data (e.g. ��ip�� and ��port��)
                    string lobbyId = lobby.Id;
                    string serverIp = lobby.Data != null && lobby.Data.ContainsKey("ip")
                        ? lobby.Data["ip"].Value
                        : "Unknown";
                    string serverPort = lobby.Data != null && lobby.Data.ContainsKey("port")
                        ? lobby.Data["port"].Value
                        : "Unknown";

                    Debug.Log($"Lobby ID: {lobbyId} - Server IP: {serverIp} | Port: {serverPort}");
                    isServerAvaliable = true;
                }
            }
            else
            {
                Debug.Log("NO AVALIABLE SERVER CURRENTLY");
            }
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError("Error querying Lobby:" + ex.Message);
        }
    }
}
