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

    public bool isPrivate = false;
    public String lobbyName;

    bool isDeallocating = false;
    public string backfillIpAddress;
    public ushort backfillPort;
    public bool isServerAvailable = false;
    public MultiplayManager mpManager;
    public AuthenticationManager authManager;

    public LobbyManager lbyManager;

    public async void Start()
    {
        mpManager = GetComponent<MultiplayManager>();
        lbyManager = GetComponent<LobbyManager>();
        authManager = GetComponent<AuthenticationManager>();
        await UnityServices.InitializeAsync();

        while (!authManager.isSignedIn)
        {
            await Task.Delay(1000);
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

#endif
    }

    public async Task<Tuple<string, ushort>> AllocateServer(string playerId, string galleryId)
    {
        var players = new List<Unity.Services.Matchmaker.Models.Player> { new(playerId, new Dictionary<string, object>()) };
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

        return new Tuple<string, ushort>(allocatedIpAddress, allocatedPort);

    }

}