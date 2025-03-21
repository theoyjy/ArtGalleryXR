using UnityEngine;
using UnityEngine.UI;

public class LobbyCanvasControls : MonoBehaviour
{
    private GameObject lobbyUIPanel;
    private GameObject createGalleryUIPanel;
    private Button createGalleryButton; // In lobbyUIPanel
    private Button exitCreateGalleryButton; // In createGalleryUIPanel

    void Start()
    {
        // Set UI references (panels are siblings of LobbyUIManager)
        lobbyUIPanel = transform.parent.Find("LobbyPanel").gameObject;
        if (!lobbyUIPanel)
            Debug.LogError("NO LOBBY UI");
        createGalleryUIPanel = transform.parent.Find("CreateGalleryPanel").gameObject;
        if (!createGalleryUIPanel)
            Debug.LogError("NO CREATE GALLERY UI");
        // Set Button reference (Button is child of lobbyUIPanel)
        createGalleryButton = lobbyUIPanel.transform.Find("CreateNewGalleryButton").GetComponent<Button>();
        if (!createGalleryButton)
            Debug.LogError("NO CREATE GALLERY BUTTON");
        // Set Button reference (Button is child of createGalleryUIPanel)
        exitCreateGalleryButton = createGalleryUIPanel.transform.Find("ExitButton").GetComponent<Button>();
        if (!exitCreateGalleryButton)
            Debug.LogError("NO EXIT BUTTON");

        // Attach button click listener
        createGalleryButton.onClick.AddListener(ShowCreateGalleryUI);
        exitCreateGalleryButton.onClick.AddListener(ShowLobbyUI);
    }
    private void ShowCreateGalleryUI()
    {
        lobbyUIPanel.SetActive(false);
        createGalleryUIPanel.SetActive(true);
    }

    private void ShowLobbyUI()
    {
        createGalleryUIPanel.SetActive(false);
        lobbyUIPanel.SetActive(true);
    }
}
