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
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class LobbyPanelControls : MonoBehaviour
{
    // Reference to the LobbyManager GameObject
    public LobbyManager lobbyManager;

    // <Scroll Rects>
    public Button buttonPrefab;
    public RectTransform availableGalleriesScrollTransform;
    public RectTransform yourGalleriesScrollTransform;
    // </Scroll Rects>

    // <Buttons>
    public Button refreshGalleriesButton;
    public Button logoutButton;
    public Button createGalleryButton;
    public Button exitApplicationButton;
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

    // <Join/Delete Gallery>
    private GameObject joinOrDeleteGalleryUIPanel;
    private JoinOrDeleteGalleryControls joinOrDeleteGalleryControls;
    // </Join/Delete Gallery>

    // <Are You Sure>
    private GameObject areYouSureUIPanel;
    private AreYouSureControls areYouSureControls;
    // </Are You Sure>

    private async void Start()
    {
        // Attach click events to the buttons
        refreshGalleriesButton = transform.Find("RefreshGalleriesButton").GetComponent<Button>();
        if (refreshGalleriesButton == null)
            Debug.Log("LOBBY PANEL: NO REFRESH GALLERIES BUTTON");
        else
            refreshGalleriesButton.onClick.AddListener(OnRefreshGalleriesClicked);

        logoutButton = transform.Find("LogoutButton").GetComponent<Button>();
        if (logoutButton == null)
            Debug.Log("LOBBY PANEL: NO LOGOUT BUTTON");
        else
            logoutButton.onClick.AddListener(OnLogoutClicked);

        createGalleryButton = transform.Find("CreateNewGalleryButton").GetComponent<Button>();
        if (createGalleryButton == null)
            Debug.Log("LOBBY PANEL: NO CREATE GALLERY BUTTON");
        else
            createGalleryButton.onClick.AddListener(OnCreateGalleryClicked);

        exitApplicationButton = transform.Find("ExitApplicationButton").GetComponent<Button>();
        if (logoutButton == null)
            Debug.Log("LOBBY PANEL: NO LOGOUT BUTTON");
        else
            exitApplicationButton.onClick.AddListener(OnExitApplicationClicked);

        enterPasswordUIPanel = transform.parent.Find("EnterPasswordPanel").gameObject;
        if (enterPasswordUIPanel == null)
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

        // Join/delete gallery window
        joinOrDeleteGalleryUIPanel = transform.parent.Find("JoinOrDeleteGalleryPanel").gameObject;
        if (!joinOrDeleteGalleryUIPanel)
            Debug.LogError("NO JOIN/DELETE GALLERY GAME OBJECT");
        joinOrDeleteGalleryControls = joinOrDeleteGalleryUIPanel.GetComponent<JoinOrDeleteGalleryControls>();
        if (!joinOrDeleteGalleryControls)
            Debug.LogError("NO JOIN/DELETE GALLERY CONTROLS");
        joinOrDeleteGalleryUIPanel.SetActive(false);

        // Are you sure window
        areYouSureUIPanel = transform.parent.Find("AreYouSurePanel").gameObject;
        if (!areYouSureUIPanel)
            Debug.LogError("NO ARE YOU SURE GAME OBJECT");
        areYouSureControls = areYouSureUIPanel.GetComponent<AreYouSureControls>();
        if (!areYouSureControls)
            Debug.LogError("NO ARE YOU SURE CONTROLS");
        areYouSureUIPanel.SetActive(false);
        
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

        // Refresh list at start up
        await Task.Delay(2000);
        OnRefreshGalleriesClicked();
    }
    private void OnRefreshGalleriesClicked()
    {
        ClearContent(availableGalleriesScrollTransform);
        ClearContent(yourGalleriesScrollTransform);
        Debug.Log("ACK: Clicked on refresh galleries button");

        // 1. Get all database galleries
        List<GalleryDetail> allDatabaseGalleries = new List<GalleryDetail>();
        SharedDataManager.GetAllGalleries(
            onSuccess: async (List<GalleryDetail> Galleries) =>
            {
                allDatabaseGalleries = Galleries;
                Debug.Log("Successfull retrieved all galleris from database. Total galleries count: " + Galleries.Count);
                // Testing
                string galleriesInfo = "Username:\t\t\tGallery:\n";
                for (int i = 0; i < allDatabaseGalleries.Count; i++)
                {
                    galleriesInfo += i.ToString() + ". " + allDatabaseGalleries[i].OwnID + "\t\t\t" + allDatabaseGalleries[i].GalleryID + "\n";
                }
                Debug.LogWarning(galleriesInfo);

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
            },
            onError: (PlayFabError error) =>
            {
                Debug.LogError("Get public Galleries failed: " + error.GenerateErrorReport());
                return;
            }
        );
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
        newButton.onClick.AddListener(() => 
        {
            // Setup join/delete window: change title to gallery name, connect join/delete buttons appropriately
            joinOrDeleteGalleryControls.updateTitleText(gallery.GalleryID);

            // If join clicked
            joinOrDeleteGalleryControls.joinButton.onClick.AddListener(async () =>
            {
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

            // If delete clicked
            joinOrDeleteGalleryControls.deleteButton.onClick.AddListener(async () =>
            {
                SharedDataManager.DeleteGalleryByID(gallery.GalleryID);
                joinOrDeleteGalleryControls.closeJoinDeleteWindow();

                messageBoxControls.updateMessageBoxText(gallery.GalleryID + " deleted successfully!");
                messageBoxControls.showMessageBox();

                // Refresh list
                await Task.Delay(2000);
                OnRefreshGalleriesClicked();
            });

            joinOrDeleteGalleryControls.showJoinOrDeleteWindow();
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
        
        // Check if public or private to either join immediately or pass to password window
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

        // Setup are you sure window: set title and text message, remove current listener on yes button, add new ones for yes and no
        areYouSureControls.updateTitleText("Confirm Logout");
        areYouSureControls.updateMessageText("Are you sure you want to logout?");
        areYouSureControls.removeCurrentListeners();
        areYouSureControls.yesButton.onClick.AddListener(() =>
        {
            Debug.Log("Logging user: " + SharedDataManager.CurrentUserName + " out...");
            PlayFabClientAPI.ForgetAllCredentials();
            SharedDataManager.CurrentUserName = "";
#if UNITY_ANDROID
            SceneManager.LoadScene("LoginUI");
#else
            SceneManager.LoadScene("Login");
#endif
        });
        // No button already programmed to close window

        // Display
        areYouSureControls.showAreYouSureWindow();
    }

    private void OnExitApplicationClicked()
    {
        Debug.Log("ACK: Clicked on exit application button");

        // Setup are you sure window: set title and text message, remove current listener on yes button, add new ones for yes and no
        areYouSureControls.updateTitleText("Confirm Exit");
        areYouSureControls.updateMessageText("Are you sure you want to exit the application?");
        areYouSureControls.removeCurrentListeners();
        areYouSureControls.yesButton.onClick.AddListener(() =>
        {
            Debug.Log("Exiting the application...");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
            // NEED TO HANDLE ANYTHING ELSE BEFORE THIS? LOGOUT?
            Application.Quit();
        });
        // No button already programmed to close window

        // Display
        areYouSureControls.showAreYouSureWindow();
    }

    private void OnCreateGalleryClicked()
    {
        Debug.Log("ACK: Clicked on create new gallery button");
    }
}
