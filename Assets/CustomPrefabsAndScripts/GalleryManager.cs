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
    public bool playerIsHost;
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

    public async Task GoToLobbies()
    {
        await SceneManager.LoadSceneAsync("Lobby");
        Scene lobbyScene = SceneManager.GetSceneByName("Lobby");
        SceneManager.SetActiveScene(lobbyScene);
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
        await GoToLobbies();
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
#if !SERVER_BUILD
        // if (!isOpen)
        // {
        //     LeaveGallery();
        // }
#endif
    }

    public async Task LoadGalleryState()
    {
        // Load the gallery state from the current lobby
        Debug.Log("Loading gallery state from lobby: " + currentLobby.Name);
    }
}