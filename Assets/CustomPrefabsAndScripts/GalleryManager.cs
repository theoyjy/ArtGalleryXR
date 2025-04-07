using UnityEngine;
using Unity.Services.Lobbies.Models;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;
using System;
using System.Collections;

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

    public GameObject hooks;
    private List<CanvasUpdater> allCanvasUpdaters = new List<CanvasUpdater>();

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

        if (hooks == null)
        {
            Debug.LogError("Hooks object not assigned!");
            return;
        }

        foreach (Transform hook in hooks.transform)
        {
            Transform canvasTransform = hook.Find("painting/canvas");
            if (canvasTransform != null)
            {
                CanvasUpdater updater = canvasTransform.GetComponent<CanvasUpdater>();
                if (updater != null)
                {
                    allCanvasUpdaters.Add(updater);
                    Debug.Log("Found CanvasUpdater on " + canvasTransform.name);
                }
                else
                {
                    Debug.LogWarning("No CanvasUpdater on " + canvasTransform.name);
                }
            }
            else
            {
                Debug.LogWarning("No canvas under " + hook.name);
            }
        }

        Debug.Log("Total CanvasUpdaters found: " + allCanvasUpdaters.Count);
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
        # if !SERVER_BUILD
        if (!isOpen)
        {
            LeaveGallery();
        }
        #endif
    }

    public async Task LoadGalleryState()
    {
        SharedDataManager.GetGallery(galleryId, OnGalleryLoadedToSetHook, OnGalleryError);
        // Load the gallery state from the current lobby
        Debug.Log("Loading gallery state from lobby: " + galleryId);//galleryId
    }

    IEnumerator WaitAndDo(Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }

    void OnGalleryLoadedToSetHook(GalleryDetail gallery)
    {
        if (gallery == null || gallery.Canvas == null || gallery.Canvas.Count == 0)
        {
            Debug.LogWarning("GalleryDetail has no Canvas data.");
            return;
        }

        int index = 0;
        foreach (CanvasUpdater Updater in allCanvasUpdaters)
        {
            string url = gallery.Canvas[index];
            if (string.IsNullOrEmpty(url))
            {
                Debug.LogWarning($" {index}  URL is empty，pass.");
                index++;
                continue;
            }

            Updater.texture_url = url;
            Updater.is_Changed = true;
            Debug.Log($"[{Updater.gameObject.name}] 设置 CanvasUpdater.texture_url 为：{url}");

            index++;
        }

        Debug.Log($"Update finished {index}  CanvasUpdater。");
    }

    void OnGalleryError(PlayFabError error)
    {
        Debug.LogError("Get Gallery DATA FAILED：" + error?.ErrorMessage);
    }

}
