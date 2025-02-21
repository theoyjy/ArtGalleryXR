using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

// Running On server only
public class ServerLobbyRegistration : MonoBehaviour
{
    // Supposed we've already
    public string serverIp = "123.456.789.0"; 
    public int serverPort = 7777;            
    public int maxPlayers = 10; 

    async void Start()
    {
        // Init Unity Gaming Services
        await UnityServices.InitializeAsync();
        // If authorized required, do it
        await AuthenticationService.Instance.SignInAnonymouslyAsync();

        // register to  Lobby server
        await RegisterServerLobby();
    }

    async Task RegisterServerLobby()
    {
        try
        {
            
            Dictionary<string, DataObject> lobbyData = new Dictionary<string, DataObject>
            {
                { "ip", new DataObject(DataObject.VisibilityOptions.Public, serverIp) },
                { "port", new DataObject(DataObject.VisibilityOptions.Public, serverPort.ToString()) }
            };

            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = false,
                Data = lobbyData
            };

            // create a lobby with Customized name
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync("TestGameServer", maxPlayers, options);
            Debug.Log("Lobby createdID" + lobby.Id);
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError("Lobby goes ERROR when created" + ex.Message);
        }
    }
}
