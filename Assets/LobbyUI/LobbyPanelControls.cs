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

    //temp code for login
    public bool IfLogin;

    // Reference to the LobbyManager GameObject
    public LobbyManager lobbyManager;

    /*************** BUTTONS ********************/
    // Reference to refresh lobbies button
    public Button refreshGalleriesButton;

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
        //tempcode for login
        IfLogin = false;
        Login();

        // Set lobby manager
        //lobbyManager = GetComponent<LobbyManager>();
        //if (!lobbyManager)
        //    Debug.LogError("Lobby Panel: no lobby manager found");

        // Attach click events to the buttons
        refreshGalleriesButton = transform.Find("RefreshGalleriesButton").GetComponent<Button>();
        refreshGalleriesButton.onClick.AddListener(OnRefreshGalleriesClicked);

        profileButton = transform.Find("ProfileButton").GetComponent<Button>();
        profileButton.onClick.AddListener(OnProfileClicked);

        logoutButton = transform.Find("LogoutButton").GetComponent<Button>();
        logoutButton.onClick.AddListener(OnLogoutClicked);

        createGalleryButton = transform.Find("CreateNewGalleryButton").GetComponent<Button>();
        createGalleryButton.onClick.AddListener(OnCreateGalleryClicked);
    }
    private void OnRefreshGalleriesClicked()
    {
        Debug.Log("ACK: Clicked on refresh galleries button");
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
        if (!IfLogin)
        {
            Login();
        }

    }

    //playfab login
    void Login()
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        };

        PlayFabClientAPI.LoginWithCustomID(request, result =>
        {
            Debug.Log("PlayFab login success?");
            IfLogin = true;
            SharedDataManager.CurrentUserName = SystemInfo.deviceUniqueIdentifier;
            // ???????????
            //TestUpdateSharedGroupData();
        }, error =>
        {
            Debug.LogError("PlayFab login failed: " + error.ErrorMessage);
        });
    }

    //public RectTransform contentPanel;   // The ScrollRect's content panel
    //public Button buttonPrefab;          // Assign a Button prefab with TMP_Text

    //// Call this function to add buttons dynamically
    //public void AddLobbyToList(string buttonText)
    //{
    //    // Instantiate a button
    //    Button newButton = Instantiate(buttonPrefab, contentPanel);

    //    // Set button dimensions (180x32 pixels)
    //    RectTransform rt = newButton.GetComponent<RectTransform>();
    //    rt.sizeDelta = new Vector2(180, 32);

    //    // Set the button text
    //    TMP_Text text = newButton.GetComponentInChildren<TMP_Text>();
    //    text.text = buttonText;

    //    // Optionally, add button click listener here
    //    newButton.onClick.AddListener(() => {
    //        Debug.Log("Button clicked: " + buttonText);
    //    });
    //}
}
