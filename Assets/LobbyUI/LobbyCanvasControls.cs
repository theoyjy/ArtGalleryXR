using UnityEngine;
using UnityEngine.UI;

public class LobbyCanvasControls : MonoBehaviour
{
    // Lobby UI
    private GameObject lobbyUIPanel;
    private LobbyPanelControls lobbyControls;

    // Create gallery UI
    private GameObject createGalleryUIPanel;
    private CreateGalleryPanelControls createGalleryControls;

    // Enter password UI
    private GameObject enterPasswordUIPanel;
    private EnterPasswordPanelControls enterPasswordControls;

    // Used to keep track of active panel (makes changing between them easier than if/elses)
    private GameObject activeUIPanel; 

    private Button createGalleryButton; // In lobbyUIPanel
    private Button exitCreateGalleryButton; // In createGalleryUIPanel
    private Button exitEnterPasswordButton; // In enterPasswordUIPanel
    private Button createNewGalleryButton; // In createGalleryUIPanel

    // Test

    void Start()
    {
        // Set UI references (panels are siblings of LobbyUIManager)
        lobbyUIPanel = transform.parent.Find("LobbyPanel").gameObject;
        if (!lobbyUIPanel)
            Debug.LogError("NO LOBBY UI");
        lobbyControls = lobbyUIPanel.GetComponent<LobbyPanelControls>();
        if (!lobbyControls)
            Debug.LogError("NO LOBBY CONTROLS");

        createGalleryUIPanel = transform.parent.Find("CreateGalleryPanel").gameObject;
        if (!createGalleryUIPanel)
            Debug.LogError("NO CREATE GALLERY UI");
        createGalleryControls = createGalleryUIPanel.GetComponent<CreateGalleryPanelControls>();
        if (!createGalleryControls)
            Debug.LogError("NO CREATE GALLERY CONTROLS");

        enterPasswordUIPanel = transform.parent.Find("EnterPasswordPanel").gameObject;
        if (!enterPasswordUIPanel)
            Debug.LogError("NO ENTER PASSWORD UI");
        enterPasswordControls = enterPasswordUIPanel.GetComponent<EnterPasswordPanelControls>();
        if (!enterPasswordControls)
            Debug.LogError("NO ENTER PASSWORD CONTROLS");

        // Set Button reference (Button is child of lobbyUIPanel)
        createGalleryButton = lobbyUIPanel.transform.Find("CreateNewGalleryButton").GetComponent<Button>();
        if (!createGalleryButton)
            Debug.LogError("NO CREATE GALLERY BUTTON");

        // Set Button reference (Button is child of lobbyUIPanel)
        createNewGalleryButton = createGalleryUIPanel.transform.Find("CreateGalleryButton").GetComponent<Button>();
        if (!createNewGalleryButton)
            Debug.LogError("NO CREATE NEW GALLERY BUTTON");

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
        createNewGalleryButton.onClick.AddListener(ShowLobbyUI);

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

        // Clear fields of previous window (not overwriting GameObject to make this inheritable)
        enterPasswordControls.ClearAllFields();
    }

    private void ShowLobbyUI()
    {
        activeUIPanel.SetActive(false);
        activeUIPanel = lobbyUIPanel;
        activeUIPanel.SetActive(true);

        // Clear fields of previous window (not overwriting GameObject to make this inheritable)
        enterPasswordControls.ClearAllFields();
        createGalleryControls.ClearAllFields();
    }

    private void ShowEnterPasswordUI()
    {
        activeUIPanel.SetActive(false);
        activeUIPanel = enterPasswordUIPanel;
        activeUIPanel.SetActive(true);

        // Clear fields of previous window (not overwriting GameObject to make this inheritable)
        enterPasswordControls.ClearAllFields();
        createGalleryControls.ClearAllFields();
    }
}
