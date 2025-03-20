using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class MainMenuUIControls : MonoBehaviour
{
    // Reference to the LobbyManager GameObject
    //public GameObject lobbyManager;

    /*************** BUTTONS ********************/
    // Reference to refresh public lobbies button
    public Button refreshPublicGalleriesButton;

    // Reference to refresh private button
    public Button refreshPrivateGalleriesButton;

    // Reference to profile button
    public Button profileButton;

    // Reference to logout button
    public Button logoutButton;
    /*******************************************/

    /*************** INPUT FIELDS ********************/

    private void Start()
    {
        // Attach click event to the button
        refreshPublicGalleriesButton = transform.Find("RefreshPublicGalleriesButton").GetComponent<Button>();
        refreshPublicGalleriesButton.onClick.AddListener(OnRefreshPublicClicked);

        refreshPrivateGalleriesButton = transform.Find("RefreshPrivateGalleriesButton").GetComponent<Button>();
        refreshPrivateGalleriesButton.onClick.AddListener(OnRefreshPrivateClicked);

        profileButton = transform.Find("ProfileButton").GetComponent<Button>();
        profileButton.onClick.AddListener(OnProfileClicked);

        logoutButton = transform.Find("LogoutButton").GetComponent<Button>();
        logoutButton.onClick.AddListener(OnLogoutClicked);
    }
    private void OnRefreshPublicClicked()
    {
        // Retrieve lobby name from input field
        //string lobbyName = lobbyNameInputField.text;
        //
        //if (string.IsNullOrEmpty(lobbyName))
        //{
        //    Debug.LogWarning("Lobby name cannot be empty.");
        //    return;
        //}
        //
        //// Call JoinLobby function from LobbyManager
        //lobbyManager.GetComponent<LobbyManager>().JoinLobby(lobbyName);
    }
    private void OnRefreshPrivateClicked()
    {
        // Clear list of 

        // Get new list of private lobbies
        
        // Iterate over list and see if 
    }
    private void OnProfileClicked()
    {
        // Open profile UI
        // profileReference.Open();
    }
    private void OnLogoutClicked()
    {
        // Opens are you sure dialog
        // areYouSure.Open();
    }
}
