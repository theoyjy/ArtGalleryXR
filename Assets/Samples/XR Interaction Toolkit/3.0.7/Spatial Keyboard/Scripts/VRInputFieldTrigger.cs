using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

//#if UNITY_ANDROID
public class VRInputFieldTrigger : MonoBehaviour, IPointerClickHandler
{
    public TMP_InputField inputField; // 当前输入框（账号或密码）
    public VRKeyboardManager keyboardManager; // VR 键盘管理器

    public void OnPointerClick(PointerEventData eventData)
    {
        if (keyboardManager != null)
        {
            keyboardManager.SetKeyboardTarget(inputField); // 🎯 绑定 XRI Keyboard 到当前输入框
        }
    }
}

//#endif // UNITY_ANDROID
