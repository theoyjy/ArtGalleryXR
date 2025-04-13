using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class AreYouSureControls : MonoBehaviour
{
    private TMP_Text titleText;
    private TMP_Text messageText;
    public Button yesButton;
    public Button noButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        titleText = transform.Find("TitleText").GetComponent<TMP_Text>();
        if (titleText == null)
            Debug.LogError("NO TITLE TEXT FOR ARE YOU SURE");

        messageText = transform.Find("MessageText").GetComponent<TMP_Text>();
        if (messageText == null)
            Debug.LogError("NO MESSAGE TEXT FOR ARE YOU SURE");

        yesButton = transform.Find("YesButton").GetComponent<Button>();
        if (yesButton == null)
            Debug.LogError("NO YES BUTTON FOR ARE YOU SURE");
        // Yes button handling done in file that calls this window

        noButton = transform.Find("NoButton").GetComponent<Button>();
        if (noButton == null)
            Debug.LogError("NO NO BUTTON FOR ARE YOU SURE");
        else
            noButton.onClick.AddListener(closeAreYouSureWindow);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void removeCurrentListeners()
    {
        yesButton.onClick.RemoveAllListeners();
    }

    public void updateTitleText(string title)
    {
        titleText.text = title;
    }

    public void updateMessageText(string message)
    {
        messageText.text = message;
    }

    public void showAreYouSureWindow()
    {
        transform.gameObject.SetActive(true);
    }
    public void closeAreYouSureWindow()
    {
        transform.gameObject.SetActive(false);
    }
}
