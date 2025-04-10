using UnityEngine;
using Unity.Collections;
//using Unity.Services.Multiplay;
using Unity.Services.Core;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
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
using Unity.Services.Multiplayer;

public class MatchmakerManager : MonoBehaviour
{
    public bool isAllocated = false;
    public string allocatedIpAddress;
    public ushort allocatedPort;

    bool isDeallocating = false;
    public string backfillIpAddress;
    public MultiplayManager mpManager;
    public AuthenticationManager authManager;

    public async void Start()
    {
        mpManager = GetComponent<MultiplayManager>();
        authManager = GetComponent<AuthenticationManager>();
        if (!SharedDataManager.isAuthenticated)
        {
            await UnityServices.InitializeAsync();

            while (!authManager.isSignedIn)
            {
                await Task.Delay(1000);
            }
        }
    }

    async void Update()
    {
    }

    public async Task<Tuple<string, ushort>> AllocateServer(string playerId, string galleryId)
    {
        isAllocated = false;
        allocatedIpAddress = null;
        allocatedPort = 0;
        CreateTicketResponse ticketResponse = null;

        try
        {
            var players = new List<Unity.Services.Matchmaker.Models.Player> { new(playerId, new Dictionary<string, object>()) };
            var options = new CreateTicketOptions("Lobbies", new Dictionary<string, object>());
            ticketResponse = await MatchmakerService.Instance.CreateTicketAsync(players, options);
            Debug.Log("New Ticket Created: " + ticketResponse.Id);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }


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

        // mpManager.ipAddress = allocatedIpAddress;
        // mpManager.port = (ushort)allocatedPort;
        // mpManager.hasServerData = isAllocated;

        return new Tuple<string, ushort>(allocatedIpAddress, allocatedPort);

    }

}