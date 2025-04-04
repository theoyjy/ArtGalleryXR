using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;

public class CreateGalleryPanelControls : MonoBehaviour
{
    // These two are handled in editor
    public LobbyManager lobbyManager;
    //public SharedDataManager sharedDataManager;

    // These are set in Start()
    private Button exitButton;
    private Button createGalleryButton;
    private Button eyeButton;
    private bool eyeIsOpen;
    public Sprite eyeOpenIcon;
    public Sprite eyeClosedIcon;

    private TMP_InputField galleryNameIF;
    private TMP_InputField galleryPasswordIF;
    private TMP_Text galleryPasswordText;

    private Toggle privateToggle;
    private TMP_Text maxPlayersText;
    private Slider maxPlayersSlider;

    void Start()
    {
        // Init references
        exitButton = transform.Find("ExitButton").GetComponent<Button>();
        if (!exitButton)
            Debug.LogError("Canvas Panel Controls: No exit button found");
        // Exit handled by LobbyCanvasControls

        createGalleryButton = transform.Find("CreateGalleryButton").GetComponent<Button>();
        if (!createGalleryButton)
            Debug.LogError("Canvas Panel Controls: No create gallery button found");
        else
            createGalleryButton.onClick.AddListener(OnCreateGalleryClicked);
        eyeButton = transform.Find("EyeButton").GetComponent<Button>();
        if (!eyeButton)
            Debug.LogError("Canvas Panel Controls: No eye button found");
        else
            eyeButton.onClick.AddListener(OnEyeButtonClicked);

        galleryNameIF = transform.Find("GalleryNameInputField").GetComponent<TMP_InputField>();
        if (!galleryNameIF)
            Debug.LogError("Canvas Panel Controls: No gallery name IF found");

        galleryPasswordIF = transform.Find("PasswordInputField").GetComponent<TMP_InputField>();
        if (!galleryPasswordIF)
            Debug.LogError("Canvas Panel Controls: No gallery password IF found");

        // Hide password as default
        galleryPasswordIF.contentType = TMP_InputField.ContentType.Password;
        eyeButton.image.sprite = eyeClosedIcon;
        eyeIsOpen = false;

        galleryPasswordText = transform.Find("PasswordText").GetComponent<TMP_Text>();
        if (!galleryPasswordText)
            Debug.LogError("Canvas Panel Controls: No password text found");

        privateToggle = transform.Find("PrivateToggle").GetComponent<Toggle>();
        if (!privateToggle)
            Debug.LogError("Canvas Panel Controls: No private toggle found");
        else
            privateToggle.onValueChanged.AddListener(ToggleValueChanged);

        maxPlayersText = transform.Find("MaxPlayersText").GetComponent<TMP_Text>();
        if (!maxPlayersText)
            Debug.LogError("Canvas Panel Controls: No max players text found");

        maxPlayersSlider = transform.Find("MaxPlayersSlider").GetComponent<Slider>();
        if (!maxPlayersSlider)
            Debug.LogError("Canvas Panel Controls: No max players slider found");
        else
            maxPlayersSlider.onValueChanged.AddListener(SliderValueChanged);
    }

    // Update is called once per frame
    void Update()
    {
        //Debug.Log($"content: '{galleryNameIF.text}'");
    }

    private void OnCreateGalleryClicked()
    {
        Debug.Log("OnCreateGalleryClicked");
        Debug.Log($"contentONCreate: '{galleryNameIF.text}'");
        string galleryName = galleryNameIF.text;
        int maxPlayers = (int)maxPlayersSlider.value;
        bool isPrivate = privateToggle.isOn;
        string password = galleryPasswordIF.text;

        if (string.IsNullOrEmpty(galleryName))
        {
            TMP_Text namePlaceholder = galleryNameIF.placeholder.GetComponent<TMP_Text>();
            galleryNameIF.text = "";
            namePlaceholder.text = "SET A GALLERY NAME";
            return;
        }

        if (isPrivate)
        {
            TMP_Text passwordPlaceholder = galleryPasswordIF.placeholder.GetComponent<TMP_Text>();
            if (string.IsNullOrEmpty(password))
            {
                galleryPasswordIF.text = "";
                passwordPlaceholder.text = "SET PASSWORD";
                return;
            }
            else if (password.Length < 8)
            {
                galleryPasswordIF.text = "";
                passwordPlaceholder.text = "PW > 8 CHARACTERS";
                return;
            }
            else if (password.Length > 64)
            {
                galleryPasswordIF.text = "";
                passwordPlaceholder.text = "PW < 64 CHARACTERS";
                return;
            }
        }

        CheckAndCreateGallery(galleryName, maxPlayers, isPrivate, password);
    }
    private void CheckAndCreateGallery(string galleryName, int maxPlayers, bool isPrivate, string password)
    {
        SharedDataManager.GetAllGalleries(
            onSuccess: async (List<GalleryDetail> Galleries) =>
            {
                Debug.Log("public Galleries count: " + Galleries.Count);
                bool sameNameExist = false;
                foreach (var gallery in Galleries)
                {
                    Debug.Log($"ID: {gallery.GalleryID}, 名称: {gallery.LobbyID}, 权限: {gallery.permission}");
                    if (galleryName.Equals(gallery.LobbyID))
                    {
                        Debug.Log("The name has existed! Change to another one.");
                        sameNameExist = true;
                        break;
                    }
                }

                if (sameNameExist)
                {
                // 可选：设置提示 UI
                return;
                }

                Debug.Log("Attempting to create gallery...\n" +
                    "Name: " + galleryName + "\n" +
                    "Max Players: " + maxPlayers + "\n" +
                    "Private: " + isPrivate + "\n" +
                    "Password: " + password + "\n");

                string username = SharedDataManager.CurrentUserName;

                string lobbyID = await lobbyManager.CreateLobby(galleryName, username, maxPlayers, isPrivate, password);
                SharedDataManager.CreateGallery(galleryName, lobbyID, !isPrivate);

                //SharedDataManager.AddCanva(galleryName, "https://canva.link/xxx",
                //onSuccess: result => Debug.Log("set successful: " + result),
                //onError: error => Debug.LogError("set failed: " + error.ErrorMessage));
            },
            onError: (PlayFabError error) =>
            {
                Debug.LogError("Get public Galleries failed: " + error.GenerateErrorReport());
            }
        );
    }

    private void ToggleValueChanged(bool value)
    {
        // True = private, false = public
        if (value)
        {
            galleryPasswordText.gameObject.SetActive(true);
            galleryPasswordIF.gameObject.SetActive(true);
            eyeButton.gameObject.SetActive(true);
        }
        else
        {
            galleryPasswordText.gameObject.SetActive(false);
            galleryPasswordIF.gameObject.SetActive(false);
            eyeButton.gameObject.SetActive(false);
        }
    }

    private void OnEyeButtonClicked()
    {
        eyeIsOpen = !eyeIsOpen;
        if (eyeIsOpen)
        {
            eyeButton.image.sprite = eyeOpenIcon;
            galleryPasswordIF.contentType = TMP_InputField.ContentType.Standard;
        }
        else
        {
            eyeButton.image.sprite = eyeClosedIcon;
            galleryPasswordIF.contentType = TMP_InputField.ContentType.Password;
        }
        galleryPasswordIF.ForceLabelUpdate();
    }

    private void SliderValueChanged(float value)
    {
        maxPlayersText.text = ((int)value).ToString();
    }

    public void ClearAllFields()
    {
        // Gallery name IF
        galleryNameIF.text = string.Empty;
        TMP_Text namePlaceholder = galleryNameIF.placeholder.GetComponent<TMP_Text>();
        namePlaceholder.text = "Gallery Name...";

        // Max players
        maxPlayersSlider.value = 1;

        // Gallery password IF
        galleryPasswordIF.text = string.Empty;
        TMP_Text passwordPlaceholder = galleryPasswordIF.placeholder.GetComponent<TMP_Text>();
        passwordPlaceholder.text = "Password...";

        // Private fields
        privateToggle.isOn = true;
        galleryPasswordIF.contentType = TMP_InputField.ContentType.Password;
        eyeButton.image.sprite = eyeClosedIcon;
        eyeIsOpen = false;
    }
}
