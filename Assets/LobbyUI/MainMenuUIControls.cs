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
    private void OnRefreshPrivateClicked()
    {
        // Clear list of existing galleries (now inactive galleries will be removed)
        // privateGalleryList.clear();

        // Get new list of private lobbies
        // allPrivateGalleries = lobbyManager.getListOfGalleries(filter private);

        // For each lobby create a button
        // for (Gallery gallery : allPrivateGalleries)
        // {
        //    galleryButton = createButton(gallery.name, gallery.players);
        //    galleryButton.addListener(joinGalleryWithLobbyID);
        //    privateGalleriesList.append(galleryButton);
        // }
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
