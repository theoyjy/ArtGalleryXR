using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;
using TMPro;
using PlayFab.ClientModels;
using PlayFab;
using System.Security.Cryptography;
public class LobbyPanelControls : MonoBehaviour
{
    // Reference to the LobbyManager GameObject
    public LobbyManager lobbyManager;

    /*************** BUTTONS ********************/
    // Reference to refresh public lobbies button
    public Button refreshPublicGalleriesButton;

    // Reference to refresh private button
    public Button refreshPrivateGalleriesButton;

    // Reference to profile button
    public Button profileButton;

    // Reference to logout button
    public Button logoutButton;

    // Reference to create gallery button
    public Button createGalleryButton;
    /*******************************************/

    /****************** PRIVATE ****************/


    /*******************************************/
    private void Start()
    {
        // Set lobby manager
        lobbyManager = GetComponent<LobbyManager>();
        if (!lobbyManager)
            Debug.LogError("Lobby Panel: no lobby manager found");

        // Attach click events to the buttons
        refreshPublicGalleriesButton = transform.Find("RefreshPublicGalleriesButton").GetComponent<Button>();
        refreshPublicGalleriesButton.onClick.AddListener(OnRefreshPublicClicked);

        refreshPrivateGalleriesButton = transform.Find("RefreshPrivateGalleriesButton").GetComponent<Button>();
        refreshPrivateGalleriesButton.onClick.AddListener(OnRefreshPrivateClicked);

        profileButton = transform.Find("ProfileButton").GetComponent<Button>();
        profileButton.onClick.AddListener(OnProfileClicked);

        logoutButton = transform.Find("LogoutButton").GetComponent<Button>();
        logoutButton.onClick.AddListener(OnLogoutClicked);

        createGalleryButton = transform.Find("CreateNewGalleryButton").GetComponent<Button>();
        createGalleryButton.onClick.AddListener(OnCreateGalleryClicked);
    }
    private void OnRefreshPublicClicked()
    {
        Debug.Log("ACK: Clicked on refresh public galleries button");
        //List<Lobby> availableLobbies = await lobbyManager.QueryAvailableLobbies();

        // Clear list of existing galleries (now inactive galleries will be removed)
        // publicGalleryList.clear();

        // Get new list of public lobbies
        // allPublicGalleries = lobbyManager.getListOfGalleries(filter public);

        // For each lobby create a button
        // for (Gallery gallery : allPublicGalleries)
        // {
        //    galleryButton = createButton(gallery.name, gallery.players);
        //    galleryButton.addListener(joinGalleryWithLobbyID);
        //    publicGalleriesList.append(galleryButton);
        // }
    }
    private void OnRefreshPrivateClicked()
    {
        Debug.Log("ACK: Clicked on refresh private galleries button");
        // Clear list of existing galleries (now inactive galleries will be removed)
        // privateGalleryList.clear();

        // Get new list of private lobbies
        // allPrivateGalleries = lobbyManager.getListOfGalleries(filter private);

        // For each lobby create a button
        // for (Gallery gallery : allPrivateGalleries)
        // {
        //    galleryButton = createButton(gallery.name, gallery.players);
        //    galleryButton.addListener(joinGalleryWithLobbyID);
        //    privateGalleriesList.append(galleryButton);
        // }
    }
    private void OnProfileClicked()
    {
        Debug.Log("ACK: Clicked on profile button");
        // Open profile UI
        // profileReference.Open();
    }
    private void OnLogoutClicked()
    {
        Debug.Log("ACK: Clicked on logout button");
        // Opens are you sure dialog
        // areYouSure.Open();
    }

    private void OnCreateGalleryClicked()
    {
        Debug.Log("ACK: Clicked on create new gallery button");
        //while (!IfLogin)
        //{
        //    Debug.Log("111111111111111111111");
        //    return;
        //}
        //Debug.Log("2222222222222222222222");

        //string ownID = "TestPlayer001";
        //string galleryName = "Test Gallery";
        //bool isPublic = true;

        //SharedDataManager.CreateNewGallery(ownID, galleryName, isPublic,
        //onSuccess: () =>
        //{
        //    Debug.Log("Gallery create success!");
        //},
        //onError: (error) =>
        //{
        //    Debug.LogError("Gallery create failed: " + error.ErrorMessage);
        //});
    }
}
