using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Samples.SpatialKeyboard; // ✅ 解决找不到 XRKeyboardDisplay
using UnityEngine.XR;

public class VRKeyboardManager : MonoBehaviour
{
    public GameObject keyboardPanel; // XRI Keyboard 面板
    public TMP_InputField accountInputField; // 账号输入框
    public TMP_InputField passwordInputField; // 密码输入框
    public XRKeyboard keyboard;
    //public XRKeyboardDisplay keyboardDisplay; // 🎯 XRI Keyboard Display 组件

    private TMP_InputField currentTargetInputField; // 当前选中的输入框

    void Start()
    {
        keyboardPanel.SetActive(false); // 默认隐藏键盘
    }

    // 🎯 让 XRI Keyboard 的 Input Field 绑定到当前选中的输入框
    public void SetKeyboardTarget(TMP_InputField selectedInputField)
    {
        keyboardPanel.SetActive(true); // 显示键盘
        if (currentTargetInputField != selectedInputField){ 

        currentTargetInputField = selectedInputField; // 记录当前输入框
        keyboardPanel.GetComponent<XRKeyboardDisplay>().inputField = currentTargetInputField;
        //keyboardDisplay.inputField = currentTargetInputField;
        keyboardPanel.GetComponent<XRKeyboard>().OverwriteText(currentTargetInputField.text);
            // 🔥 把键盘和输入框的 caretPosition 都重置到末尾
        keyboardPanel.GetComponent<XRKeyboard>().caretPosition = keyboardPanel.GetComponent<XRKeyboard>().text.Length;
        selectedInputField.caretPosition = selectedInputField.text.Length;
        }        
    }
   
}
