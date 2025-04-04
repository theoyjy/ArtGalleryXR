using System.Numerics;
using UnityEngine;
using UnityEngine.UI;

enum PanelIndices
{
    LobbyIdx = 0,
    CreateGalleryIdx = 1//,
    //EnterPasswordIdx = 2
}
public class LobbyCanvasControls : MonoBehaviour
{
    // Lobby UI
    private GameObject lobbyUIPanel;
    private LobbyPanelControls lobbyControls;

    // Create gallery UI
    private GameObject createGalleryUIPanel;
    private CreateGalleryPanelControls createGalleryControls;

    // Enter password UI
    //private GameObject enterPasswordUIPanel;
    //private EnterPasswordPanelControls enterPasswordControls;
    private GameObject[] allUIPanels;
    private int activePanelIdx;

    private Button createGalleryButton; // In lobbyUIPanel
    private Button exitCreateGalleryButton; // In createGalleryUIPanel
    //private Button exitEnterPasswordButton; // In enterPasswordUIPanel
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

        //enterPasswordUIPanel = transform.parent.Find("EnterPasswordPanel").gameObject;
        //if (!enterPasswordUIPanel)
        //    Debug.LogError("NO ENTER PASSWORD UI");
        //enterPasswordControls = enterPasswordUIPanel.GetComponent<EnterPasswordPanelControls>();
        //if (!enterPasswordControls)
        //    Debug.LogError("NO ENTER PASSWORD CONTROLS");

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
        //exitEnterPasswordButton = enterPasswordUIPanel.transform.Find("ExitButton").GetComponent<Button>();
        //if (!exitEnterPasswordButton)
        //    Debug.LogError("NO ENTER PASSWORD EXIT BUTTON");

        // Attach button click listener
        createGalleryButton.onClick.AddListener(ShowCreateGalleryUI);
        exitCreateGalleryButton.onClick.AddListener(ShowLobbyUI);
        //exitEnterPasswordButton.onClick.AddListener(ShowLobbyUI);
        //createNewGalleryButton.onClick.AddListener(ShowLobbyUI);

        // Active panel starts with lobby UI
        allUIPanels = new GameObject[]
        {
            lobbyUIPanel,
            createGalleryUIPanel//,
            //enterPasswordUIPanel
        };
        lobbyUIPanel.SetActive(true);
        createGalleryUIPanel.SetActive(false);
        //enterPasswordUIPanel.SetActive(false);
        activePanelIdx = (int)PanelIndices.LobbyIdx;
    }

    private void ChangeActivePanel(int panelIdx)
    {
        allUIPanels[activePanelIdx].SetActive(false);
        // Clear active panel but only after other connected function is completed
        activePanelIdx = panelIdx;
        allUIPanels[activePanelIdx].SetActive(true);
    }
    private void ShowCreateGalleryUI()
    {
        ChangeActivePanel((int)PanelIndices.CreateGalleryIdx);
    }

    private void ShowLobbyUI()
    {
        ChangeActivePanel((int)PanelIndices.LobbyIdx);
    }

    //public void ShowEnterPasswordUI()
    //{
    //    ChangeActivePanel((int)PanelIndices.EnterPasswordIdx);
    //}
}
