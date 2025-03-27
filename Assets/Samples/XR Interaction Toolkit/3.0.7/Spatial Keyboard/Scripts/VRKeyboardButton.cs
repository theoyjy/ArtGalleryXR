using UnityEngine;
using UnityEngine.UI;

public class VRKeyboardButton : MonoBehaviour
{
    public string keyCharacter; // °´¼ü×Ö·û
    private Button button;
    private VRKeyboardManager keyboardManager;

    void Start()
    {
        button = GetComponent<Button>();
        keyboardManager = FindObjectOfType<VRKeyboardManager>();
    }
}
