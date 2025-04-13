using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class JoinOrDeleteGalleryControls : MonoBehaviour
{
    private TMP_Text titleText;
    private Button exitButton;
    public Button joinButton;
    public Button deleteButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        titleText = transform.Find("TitleText").GetComponent<TMP_Text>();
        if (titleText == null)
            Debug.LogError("NO TITLE TEXT FOR JOIN/DELETE");

        exitButton = transform.Find("ExitButton").GetComponent<Button>();
        if (exitButton == null)
            Debug.LogError("NO EXIT BUTTON FOR JOIN/DELETE");
        else
            exitButton.onClick.AddListener(closeJoinDeleteWindow);

        joinButton = transform.Find("JoinButton").GetComponent<Button>();
        if (joinButton == null)
            Debug.LogError("NO JOIN BUTTON FOR JOIN/DELETE");
        //else
            //joinButton.onClick.AddListener(closeJoinDeleteWindow);

        deleteButton = transform.Find("DeleteButton").GetComponent<Button>();
        if (deleteButton == null)
            Debug.LogError("NO DELETE BUTTON FOR JOIN/DELETE");
        //else
            //deleteButton.onClick.AddListener(closeJoinDeleteWindow);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void updateTitleText(string galleryName)
    {
        titleText.text = galleryName;
    }

    public void showJoinOrDeleteWindow()
    {
        transform.gameObject.SetActive(true);
    }
    public void closeJoinDeleteWindow()
    {
        transform.gameObject.SetActive(false);
    }
}
