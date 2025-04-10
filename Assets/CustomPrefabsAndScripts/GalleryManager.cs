using UnityEngine;
using Unity.Services.Lobbies.Models;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
public class GalleryManager : MonoBehaviour
{
    public Lobby currentLobby;
    public string galleryId;
    public string cloudPlayerId;
    public MultiplayManager mpManager;
    public bool playerIsHost = true;
    public bool isLeaving = false;
    private bool isPingingLobby = false;
    public bool isOpen = true;
    // setting this to true will block any user interaction
    public bool isLocked = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
#if !SERVER_BUILD
        mpManager = GameObject.Find("MultiplayManager").GetComponent<MultiplayManager>();
        cloudPlayerId = AuthenticationService.Instance.PlayerId;
        currentLobby = SharedDataManager.CurrentLobby;
        galleryId = currentLobby.Name;
        await LoadGalleryState();
        isLocked = false;

        if (playerIsHost)
        {
            PingLobby();
        }
#endif
    }

    public void GoToLobbies()
    {
        // TODO: maybe try loading another scene
        SceneManager.LoadScene("Init");
    }

    public async void LeaveGallery()
    {
        if (playerIsHost)
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(currentLobby.Id);
                Debug.Log("Lobby deleted successfully.");
                SharedDataManager.ChangeLobbyID(galleryId, "LobbyID");
            }
            catch (LobbyServiceException e)
            {
                Debug.LogError($"Error deleting lobby: {e.Message}");
            }
        }
        else
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
        }
        mpManager.DisconnectFromServer();
    }

    private void OnApplicationQuit()
    {
#if !SERVER_BUILD
        LeaveGallery();
#endif
    }

// should be called periodically while inside a gallery session to keep it alive
public async void PingLobby()
{
    isPingingLobby = true;

    while (isPingingLobby && isOpen)
    {
        if (currentLobby == null) return;
        await LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
        Debug.Log("Heartbeat sent to lobby.");

        await Task.Delay(60 * 1000);
}
}

    // Update is called once per frame
    void Update()
    {
#if !SERVER_BUILD
        if (!isOpen && !isLeaving)
        {
            isLeaving = true;
            isPingingLobby = false;
            LeaveGallery();
            GoToLobbies();
        }
#endif
    }

    public async Task LoadGalleryState()
    {
        // Load the gallery state from the current lobby
        Debug.Log("Loading gallery state from lobby: " + currentLobby.Name);
    }
}