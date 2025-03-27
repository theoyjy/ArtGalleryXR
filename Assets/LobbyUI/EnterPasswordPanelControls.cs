using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class EnterPasswordPanelControls : MonoBehaviour
{
    // These are set in Start()
    private TMP_InputField galleryPasswordIF;
    private Button exitButton;
    private Button clearButton;
    private Button enterButton;
    private Button eyeButton;
    private bool eyeIsOpen;
    public Sprite eyeOpenIcon;
    public Sprite eyeClosedIcon;

    void Start()
    {
        // Init references
        exitButton = transform.Find("ExitButton").GetComponent<Button>();
        if (!exitButton)
            Debug.LogError("Enter Password Panel Controls: No exit button found");
        // Exit handled by LobbyCanvasControls

        eyeButton = transform.Find("EyeButton").GetComponent<Button>();
        if (!eyeButton)
            Debug.LogError("Enter Password Controls: No eye button found");
        else
            eyeButton.onClick.AddListener(OnEyeButtonClicked);

        // Hide password as default
        galleryPasswordIF.contentType = TMP_InputField.ContentType.Password;
        eyeButton.image.sprite = eyeClosedIcon;
        eyeIsOpen = false;

        clearButton = transform.Find("ClearButton").GetComponent<Button>();
        if (!clearButton)
            Debug.LogError("Enter Password Controls: No clear button found");
        else
            clearButton.onClick.AddListener(OnClearButtonClicked);

        enterButton = transform.Find("EnterButton").GetComponent<Button>();
        if (!enterButton)
            Debug.LogError("Enter Password Controls: No enter button found");
        else
            enterButton.onClick.AddListener(OnEnterButtonClicked);
    }

    void Update()
    {
        
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

    private void OnClearButtonClicked()
    {
        // TODO
    }

    private void OnEnterButtonClicked()
    {

    }

    public void ClearAllFields()
    {
        // Gallery password IF
        galleryPasswordIF.text = string.Empty;
        TMP_Text passwordPlaceholder = galleryPasswordIF.placeholder.GetComponent<TMP_Text>();
        passwordPlaceholder.text = "Password...";

        // Private fields
        galleryPasswordIF.contentType = TMP_InputField.ContentType.Password;
        eyeButton.image.sprite = eyeClosedIcon;
        eyeIsOpen = false;
    }
}
