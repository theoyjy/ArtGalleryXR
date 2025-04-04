using UnityEngine;
using UnityEngine.UI;

//#if UNITY_ANDROID
public class VRKeyboardButton : MonoBehaviour
{
    public string keyCharacter; // �����ַ�
    private Button button;
    private VRKeyboardManager keyboardManager;

    void Start()
    {
        button = GetComponent<Button>();
        keyboardManager = FindObjectOfType<VRKeyboardManager>();
    }
}

//#endif // UNITY_ANDROID
