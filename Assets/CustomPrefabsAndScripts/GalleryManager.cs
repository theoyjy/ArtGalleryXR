using UnityEngine;
using Unity.Services.Lobbies.Models;
using System.Security.Cryptography;
using UnityEngine.SceneManagement;
using TMPro.EditorUtilities;
using Unity.Services.Ccd.Management;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using NUnit.Framework;

public class GalleryManager : MonoBehaviour
{
    public Lobby currentLobby;
    public string galleryId;
    public string cloudPlayerId = AuthenticationService.Instance.PlayerId;
    public MultiplayManager mpManager;
    public bool playerIsHost;
    public bool isOpen = true;
    // setting this to true will block any user interaction
    public bool isLocked = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        currentLobby = SharedDataManager.CurrentLobby;
        galleryId = currentLobby.Name;
        await LoadGalleryState();
        isLocked = false;
        
        if (playerIsHost) {
            PingLobby();
        }
    }

    public async void GoToLobbies() {
        await SceneManager.LoadSceneAsync("Lobby");
        Scene galleryScene = SceneManager.GetSceneByName("Lobby");
        SceneManager.SetActiveScene(galleryScene);
    }

    public async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(currentLobby.Id, cloudPlayerId);
            Debug.Log("Successfully left the lobby.");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError($"Error leaving lobby: {e.Message}");
        }

        if (playerIsHost)
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(currentLobby.Id);
                Debug.Log("Lobby deleted successfully.");
                // TODO: call database to remove lobbyId from gallery entry
                SharedDataManager.ChangeLobbyID(galleryId, "LobbyID");
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Error deleting lobby: {e.Message}");
            }
        }

        GoToLobbies();

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
            if (currentLobby == null) return;
            await LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
            await Task.Delay(60 * 1000);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isOpen) {
            LeaveLobby();
        }
    }

    public async Task LoadGalleryState()
    {
        // Load the gallery state from the current lobby
        Debug.Log("Loading gallery state from lobby: " + currentLobby.Name);
    }
}
