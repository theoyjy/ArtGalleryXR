using UnityEngine;
using Unity.Collections;
using Unity.Services.Multiplay;
using Unity.Services.Core;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using Unity.Services.Authentication;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;


public class MatchmakerManager : MonoBehaviour
{
    public bool waitForServer = true;

    public async void Start()
    {
        if (Application.platform != RuntimePlatform.LinuxServer)
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        ClientJoin();
    }

    public async void ClientJoin()
    {
        var players = new List<Player> { new("Player1", new Dictionary<string, object>()) };
        var options = new CreateTicketOptions("Gallery-A", new Dictionary<string, object>());
        var ticketResponse = await MatchmakerService.Instance.CreateTicketAsync(players, options);

        Debug.Log(ticketResponse.Id);

        // while (waitForServer)
        // {
        //     TicketStatusResponse ticketStatusResponse = await MatchmakerService.Instance.GetTicketAsync(ticketResponse.Id);

        //     if (ticketStatusResponse.Type == typeof(MultiplayAssignment))
        //     {
        //         MultiplayAssignment assignment = (MultiplayAssignment)ticketStatusResponse.Value;
        //         if (assignment.Status == MultiplayAssignment.StatusOptions.Found)
        //         {
        //             Debug.Log("Match found! Server IP: " + assignment.Ip + " | Port: " + assignment.Port);
        //             waitForServer = false;
        //             return;
        //         }
        //     }

        //     await Task.Delay(1000);
        // }

        MultiplayAssignment assignment = null;

        do
        {
            //Rate limit delay
            await Task.Delay(TimeSpan.FromSeconds(2f));

            // Poll ticket
            var ticketStatus = await MatchmakerService.Instance.GetTicketAsync("<ticket id here>");
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
                    waitForServer = false;
                    break;
                case MultiplayAssignment.StatusOptions.InProgress:
                    //...
                    break;
                case MultiplayAssignment.StatusOptions.Failed:
                    waitForServer = true;
                    Debug.LogError("Failed to get ticket status. Error: " + assignment.Message);
                    break;
                case MultiplayAssignment.StatusOptions.Timeout:
                    waitForServer = true;
                    Debug.LogError("Failed to get ticket status. Ticket timed out.");
                    break;
                default:
                    throw new InvalidOperationException();
            }

        } while (waitForServer);

    }

}
