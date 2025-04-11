using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Lobbies.Models;
using System.Collections.Generic;
using TMPro;
using PlayFab.ClientModels;
using PlayFab;
using System.Security.Cryptography;
using PlayFab.MultiplayerModels;
using Unity.Services.Lobbies;

public class LobbyPanelControls : MonoBehaviour
{
    //temp code for login
    public bool IfLogin;

    // Reference to the LobbyManager GameObject
    public LobbyManager lobbyManager;

    // <Scroll Rects>
    public Button buttonPrefab;
    public RectTransform availableGalleriesScrollTransform;
    public RectTransform yourGalleriesScrollTransform;
    // </Scroll Rects>

    // <Buttons>
    public Button refreshGalleriesButton;
    public Button profileButton;
    public Button logoutButton;
    public Button createGalleryButton;
    // </Buttons>

    // <Enter Password Elements>
    private bool enterPasswordIsDisplayed;
    private GameObject enterPasswordUIPanel;
    private EnterPasswordPanelControls enterPasswordControls;
    private Button enterPasswordExitButton;
    private Button enterPasswordEnterButton;
    private TMP_InputField enterPasswordIF;
    // </Enter Password Elements>

    // <Message Box>
    private GameObject messageBoxUIPanel;
    private TMP_Text messageBoxText;
    private Button messageBoxCloseButton;
    private MessageBoxControls messageBoxControls;
    // </Message Box>

    /*******************************************/
    private void Start()
    {
        // Attach click events to the buttons
        refreshGalleriesButton = transform.Find("RefreshGalleriesButton").GetComponent<Button>();
        refreshGalleriesButton.onClick.AddListener(OnRefreshGalleriesClicked);

        profileButton = transform.Find("ProfileButton").GetComponent<Button>();
        profileButton.onClick.AddListener(OnProfileClicked);

        logoutButton = transform.Find("LogoutButton").GetComponent<Button>();
        logoutButton.onClick.AddListener(OnLogoutClicked);

        createGalleryButton = transform.Find("CreateNewGalleryButton").GetComponent<Button>();
        createGalleryButton.onClick.AddListener(OnCreateGalleryClicked);

        enterPasswordUIPanel = transform.parent.Find("EnterPasswordPanel").gameObject;
        if (!enterPasswordUIPanel)
            Debug.LogError("NO ENTER PASSWORD UI");
        enterPasswordControls = enterPasswordUIPanel.GetComponent<EnterPasswordPanelControls>();
        if (!enterPasswordControls)
            Debug.LogError("NO ENTER PASSWORD CONTROLS");

        messageBoxUIPanel = transform.parent.Find("MessageBoxPanel").gameObject;
        if (!messageBoxUIPanel)
            Debug.LogError("NO MESSAGE BOX UI");
        messageBoxText = messageBoxUIPanel.transform.Find("MessageText").GetComponent<TMP_Text>();
        if (!messageBoxText)
            Debug.LogError("NO MESSAGE BOX TEXT");
        messageBoxCloseButton = messageBoxUIPanel.transform.Find("CloseButton").GetComponent<Button>();
        if (!messageBoxCloseButton)
            Debug.LogError("NO MESSAGE BOX CLOSE BUTTON");
        messageBoxControls = messageBoxUIPanel.GetComponent<MessageBoxControls>();
        if (!enterPasswordControls)
            Debug.LogError("NO MESSAGE BOX CONTROLS");
        messageBoxUIPanel.SetActive(false);

        // Set Button reference (Button is child of enterPasswordUIPanel)
        enterPasswordExitButton = enterPasswordUIPanel.transform.Find("ExitButton").GetComponent<Button>();
        if (!enterPasswordExitButton)
            Debug.LogError("NO ENTER PASSWORD EXIT BUTTON");
        else
            enterPasswordExitButton.onClick.AddListener(OnEnterPasswordExitClicked);

        enterPasswordEnterButton = enterPasswordUIPanel.transform.Find("EnterButton").GetComponent<Button>();
        if (!enterPasswordEnterButton)
            Debug.LogError("NO ENTER PASSWORD ENTER BUTTON");

        enterPasswordIF = enterPasswordUIPanel.transform.Find("PasswordInputField").GetComponent<TMP_InputField>();
        if (!enterPasswordIF)
            Debug.LogError("NO ENTER PASSWORD INPUT FIELD");

        enterPasswordUIPanel.SetActive(false);
        enterPasswordIsDisplayed = false;
    }
    private async void OnRefreshGalleriesClicked()
    {
        ClearContent(availableGalleriesScrollTransform);
        Debug.Log("ACK: Clicked on refresh galleries button");

        // 1. Get all database galleries
        List<GalleryDetail> allDatabaseGalleries = new List<GalleryDetail>();
        SharedDataManager.GetAllGalleries(
            onSuccess: (List<GalleryDetail> Galleries) =>
            {
                allDatabaseGalleries = Galleries;
                Debug.Log("Successfull retrieved all galleris from database. Total galleries count: " + Galleries.Count);
            },
            onError: (PlayFabError error) =>
            {
                Debug.LogError("Get public Galleries failed: " + error.GenerateErrorReport());
                return;
            }
        );

        // 2. Get all lobbies
        List<Unity.Services.Lobbies.Models.Lobby> allAvailableLobbies = await lobbyManager.QueryAvailableLobbies();

        // 3. Find out if a gallery is yours (if it is its inactive, button connection will be to create)
        string username = SharedDataManager.CurrentUserName;
        foreach (GalleryDetail gallery in allDatabaseGalleries)
        {
            // If its yours its inactive, add it to your list and connect "createLobby" to click
            if (gallery.OwnID == username)
                AddGalleryToYourList(gallery, yourGalleriesScrollTransform);
            // Else its not yours, find out if its active. If it is, add it to the list and connect private/public check and link pw window if private
            else
            {
                // The active check works as follows: check what the database has stored for this gallery's lobby ID. Then loop through all active lobbies
                // if an active lobby ID matches the database lobby ID, then the gallery is active, add it to the list. Else if there is no match in the
                // list, the gallery is inactive so don't add it to the list
                bool isActive = false;
                string currentDatabaseLobbyID = gallery.LobbyID;
                foreach (Unity.Services.Lobbies.Models.Lobby currentLobby in allAvailableLobbies)
                {
                    string currentLobbyId = currentLobby.Id;
                    if (currentLobbyId == currentDatabaseLobbyID)
                    {
                        isActive = true; 
                        break;
                    }
                }

                // Check if a match was found
                if (isActive)
                {
                    AddGalleryToAllGalleriesList(gallery, availableGalleriesScrollTransform);
                }
            }

        }
    }

    public void ClearContent(RectTransform galleryList)
    {
        foreach (Transform child in galleryList)
        {
            Destroy(child.gameObject);
        }
    }

    private void AddGalleryToYourList(GalleryDetail gallery, RectTransform galleryList)
    {
        // Instantiate a button
        Button newButton = Instantiate(buttonPrefab, galleryList, false);

        // Set button dimensions (180x32 pixels)
        RectTransform rt = newButton.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 32);

        // Set the button text
        TMP_Text text = newButton.GetComponentInChildren<TMP_Text>();
        text.text = gallery.GalleryID;

        // Optionally, add button click listener here
        newButton.onClick.AddListener(async () => {
            Debug.Log("Attempting to create new lobby instance for the gallery: " + gallery.GalleryID);
            string username = SharedDataManager.CurrentUserName;
            bool isPrivate = gallery.permission == "public" ? false : true;
            // If its public password is just "" so call below works for both public and private
            string newLobbyId = await lobbyManager.CreateLobby(gallery.GalleryID, username, gallery.MaxPlayers, isPrivate, gallery.password);
            SharedDataManager.ChangeLobbyID(gallery.GalleryID, newLobbyId);
            // Send database new lobbyId for this gallery
            Unity.Services.Lobbies.Models.Lobby lobby = await LobbyService.Instance.GetLobbyAsync(newLobbyId);
            // Just use password from database, its already your own
            handleReturnMessageBox(await lobbyManager.JoinLobby(lobby, gallery.password, false)); // HANDLE RETURN CODE
        });
    }
    private void AddGalleryToAllGalleriesList(GalleryDetail gallery, RectTransform galleryList)
    {
        // Instantiate a button
        Button newButton = Instantiate(buttonPrefab, galleryList, false);

        // Set button dimensions (180x32 pixels)
        RectTransform rt = newButton.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 32);
        
        // Set the button text
        TMP_Text text = newButton.GetComponentInChildren<TMP_Text>();
        text.text = gallery.GalleryID;
        
        // Optionally, add button click listener here
        newButton.onClick.AddListener(async () => {
            Debug.Log("Attempting to join lobby instance for the gallery: " + gallery.GalleryID);
            Unity.Services.Lobbies.Models.Lobby lobby = await LobbyService.Instance.GetLobbyAsync(gallery.LobbyID);

            if (gallery.permission == "public")
                handleReturnMessageBox(await lobbyManager.JoinLobby(lobby, "", true)); // HANDLE JOIN STATUS RETURN
            else
            {
                enterPasswordUIPanel.SetActive(true);
                enterPasswordEnterButton.onClick.AddListener(() =>
                {
                    OnEnterPasswordEnterClicked(lobby, gallery);
                });
            }
        });
    }

    private void OnProfileClicked()
    {
        Debug.Log("ACK: Clicked on profile button");
        messageBoxControls.updateMessageBoxText("TESTING MESSAGE BOX");
        messageBoxControls.showMessageBox();
    }

    private void OnEnterPasswordEnterClicked(Unity.Services.Lobbies.Models.Lobby lobby, GalleryDetail gallery)
    {
        enterPasswordEnterButton.onClick.AddListener(async () =>
        {
            string password = enterPasswordIF.text;
            // Check valid password
            TMP_Text passwordPlaceholder = enterPasswordIF.placeholder.GetComponent<TMP_Text>();
            if (string.IsNullOrEmpty(password))
            {
                enterPasswordIF.text = "";
                passwordPlaceholder.text = "SET PASSWORD";
                return;
            }
            else if (password.Length < 8)
            {
                enterPasswordIF.text = "";
                passwordPlaceholder.text = "PW > 8 CHARACTERS";
                return;
            }
            else if (password.Length > 64)
            {
                enterPasswordIF.text = "";
                passwordPlaceholder.text = "PW < 64 CHARACTERS";
                return;
            }

            Debug.Log("ACK: Clicked on enter button. Attempting to join with PW: " + password);
            handleReturnMessageBox(await lobbyManager.JoinLobby(lobby, password, true)); // HANDLE STATUS RETURN
        });
    }

    private void handleReturnMessageBox(LobbyManager.JoinStatus returnedStatus)
    {
        switch(returnedStatus)
        {
            case LobbyManager.JoinStatus.SUCCESS:
                break;

            case LobbyManager.JoinStatus.WRONG_PASSWORD:
                messageBoxControls.updateMessageBoxText("Incorrect password entered. Please try again.");
                messageBoxControls.showMessageBox();
                break;

            case LobbyManager.JoinStatus.GALLERY_FULL:
                messageBoxControls.updateMessageBoxText("This gallery is currently full. Please try again later.");
                messageBoxControls.showMessageBox();
                break;

            case LobbyManager.JoinStatus.GALLERY_OFFLINE:
                messageBoxControls.updateMessageBoxText("This gallery is currently offline. Please try again later.");
                messageBoxControls.showMessageBox();
                break;

            default:
                break;
        }
    }

    private void OnEnterPasswordExitClicked()
    {
        enterPasswordUIPanel.SetActive(false);
        enterPasswordIsDisplayed = false;
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
        //if (!IfLogin)
        //{
        //    Login();
        //}

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
}
