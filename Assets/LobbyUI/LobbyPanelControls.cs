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
    // </Enter Password Elements>

    /*******************************************/
    private void Start()
    {
        //tempcode for login
        //IfLogin = false;
        //Login();

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
        enterPasswordUIPanel.SetActive(false);
        enterPasswordIsDisplayed = false;
    }
    private void OnRefreshGalleriesClicked()
    {
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

        // GetGalleries()
        //AddGalleryToList("Public 1", availableGalleriesScrollTransform);
        //AddGalleryToList("Public 2", availableGalleriesScrollTransform);
        //AddGalleryToList("Public 3", availableGalleriesScrollTransform);
        //AddGalleryToList("Public 4", availableGalleriesScrollTransform);
        //
        //AddGalleryToList("Own 1", yourGalleriesScrollTransform);
        //AddGalleryToList("Own 2", yourGalleriesScrollTransform);
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

    private void AddGalleryToList(GalleryDetail gallery, RectTransform galleryList)
    {
        // Instantiate a button
        Button newButton = Instantiate(buttonPrefab, galleryList, false);

        // Set button dimensions (180x32 pixels)
        RectTransform rt = newButton.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 32);
        
        // Set the button text
        TMP_Text text = newButton.GetComponentInChildren<TMP_Text>();
        text.text = gallery.GalleryName;
        
        // Optionally, add button click listener here
        newButton.onClick.AddListener(() => {
            //lobbyManager.JoinLobby();
        });
    }

    private void OnGalleryButtonClicked(string label, bool isPublic, int galleryId)
    {
        Debug.Log($"Clicked: {label}, Public: {isPublic}, ID: {galleryId}");
        // Handle logic here...
    }
    private void OnProfileClicked()
    {
        Debug.Log("ACK: Clicked on profile button");
        // Open profile UI
        // profileReference.Open();
        //enterPasswordUIPanel.SetActive(true);
        if (!enterPasswordIsDisplayed)
        {
            enterPasswordUIPanel.SetActive(true);
            enterPasswordIsDisplayed = true;
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
