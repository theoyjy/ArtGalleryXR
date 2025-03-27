using UnityEngine;
using UnityEngine.UI;

public class LobbyCanvasControls : MonoBehaviour
{
    private GameObject lobbyUIPanel;
    private GameObject createGalleryUIPanel;
    private GameObject enterPasswordUIPanel;
    private GameObject activeUIPanel; // Used to keep track of active panel (makes changing between them easier than if/elses)

    private Button createGalleryButton; // In lobbyUIPanel
    private Button exitCreateGalleryButton; // In createGalleryUIPanel
    private Button exitEnterPasswordButton; // In enterPasswordUIPanel
    
    // Test

    void Start()
    {
        // Set UI references (panels are siblings of LobbyUIManager)
        lobbyUIPanel = transform.parent.Find("LobbyPanel").gameObject;
        if (!lobbyUIPanel)
            Debug.LogError("NO LOBBY UI");
        createGalleryUIPanel = transform.parent.Find("CreateGalleryPanel").gameObject;
        if (!createGalleryUIPanel)
            Debug.LogError("NO CREATE GALLERY UI");
        enterPasswordUIPanel = transform.parent.Find("PasswordPanel").gameObject;
        if (!enterPasswordUIPanel)
            Debug.LogError("NO ENTER PASSWORD UI");

        // Set Button reference (Button is child of lobbyUIPanel)
        createGalleryButton = lobbyUIPanel.transform.Find("CreateNewGalleryButton").GetComponent<Button>();
        if (!createGalleryButton)
            Debug.LogError("NO CREATE GALLERY BUTTON");

        // Set Button reference (Button is child of createGalleryUIPanel)
        exitCreateGalleryButton = createGalleryUIPanel.transform.Find("ExitButton").GetComponent<Button>();
        if (!exitCreateGalleryButton)
            Debug.LogError("NO CREATE GALLERY EXIT BUTTON");

        // Set Button reference (Button is child of enterPasswordUIPanel)
        exitEnterPasswordButton = enterPasswordUIPanel.transform.Find("ExitButton").GetComponent<Button>();
        if (!exitEnterPasswordButton)
            Debug.LogError("NO ENTER PASSWORD EXIT BUTTON");

        // Attach button click listener
        createGalleryButton.onClick.AddListener(ShowCreateGalleryUI);
        exitCreateGalleryButton.onClick.AddListener(ShowLobbyUI);
        exitEnterPasswordButton.onClick.AddListener(ShowEnterPasswordUI);

        // Active panel starts with lobby UI
        lobbyUIPanel.SetActive(true);
        createGalleryUIPanel.SetActive(false);
        enterPasswordUIPanel.SetActive(false);
        activeUIPanel = lobbyUIPanel;
    }
    private void ShowCreateGalleryUI()
    {
        activeUIPanel.SetActive(false);
        activeUIPanel = createGalleryUIPanel;
        activeUIPanel.SetActive(true);
    }

    private void ShowLobbyUI()
    {
        activeUIPanel.SetActive(false);
        activeUIPanel = lobbyUIPanel;
        activeUIPanel.SetActive(true);
    }

    private void ShowEnterPasswordUI()
    {
        activeUIPanel.SetActive(false);
        activeUIPanel = enterPasswordUIPanel;
        activeUIPanel.SetActive(true);
    }
}
