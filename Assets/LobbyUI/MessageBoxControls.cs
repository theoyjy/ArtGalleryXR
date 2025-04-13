using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MessageBoxControls : MonoBehaviour
{
    private TMP_Text messageBoxText;
    private Button closeButton;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        closeButton = transform.Find("CloseButton").GetComponent<Button>();
        if (closeButton == null)
            Debug.LogError("NO CLOSE BUTTON FOR MESSAGE BOX");

        messageBoxText = transform.Find("MessageText").GetComponent<TMP_Text>();
        if (messageBoxText == null)
            Debug.LogError("NO TEXT FOR MESSAGE BOX");

        closeButton.onClick.AddListener(closeMessageBox);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void updateMessageBoxText(string message)
    {
        messageBoxText.text = message;
    }

    public void showMessageBox()
    {
        transform.gameObject.SetActive(true);
    }
    public void closeMessageBox()
    {
        transform.gameObject.SetActive(false);
    }
}
