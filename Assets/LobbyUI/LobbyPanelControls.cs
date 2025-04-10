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
    private void OnRefreshGalleriesClicked()
    {
        ClearContent(availableGalleriesScrollTransform);
        Debug.Log("ACK: Clicked on refresh galleries button");
        SharedDataManager.GetAllGalleries(
            onSuccess: (List<GalleryDetail> Galleries) =>
            {
                Debug.Log("Total galleries count: " + Galleries.Count);
                foreach (var gallery in Galleries)
                {
                    // Button connections handled in AddGalleryToList
                    AddGalleryToList(gallery, availableGalleriesScrollTransform);
                }
            },
            onError: (PlayFabError error) =>
            {
                Debug.LogError("Get public Galleries failed: " + error.GenerateErrorReport());
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

    private void AddGalleryToList(GalleryDetail gallery, RectTransform galleryList)
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
            Debug.Log("Gallery ID is: " + gallery.GalleryID);
            Debug.Log("Lobby ID is: " + gallery.LobbyID);
            Unity.Services.Lobbies.Models.Lobby lobby = await LobbyService.Instance.GetLobbyAsync(gallery.LobbyID);

            if (gallery.permission == "public")
                await lobbyManager.JoinLobby(lobby, "", true);
            else
            {
                enterPasswordUIPanel.SetActive(true);
                enterPasswordEnterButton.onClick.AddListener(() =>
                {
                    OnEnterPasswordEnterClicked(lobby);
                });
            }
        });
    }

    private void OnProfileClicked()
    {
        Debug.Log("ACK: Clicked on profile button");
    }

    private void OnEnterPasswordEnterClicked(Unity.Services.Lobbies.Models.Lobby lobby)
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

            // If it gets here its valid, try join
            await lobbyManager.JoinLobby(lobby, password, true);
        });
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
