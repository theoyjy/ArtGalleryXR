using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit.Samples.SpatialKeyboard;

//
// 自定义的键盘类，继承自 XRKeyboard
// 1. 提供 ForceSetText(...) 公共方法
// 2. 重写 Open(...) 函数（可选）
//
public class MyCustomXRKeyboard : XRKeyboard
{
    /// <summary>
    /// 公共方法：允许外部脚本在切换到某个新 InputField 前，
    /// 把该 InputField 的现有文本塞进键盘。
    /// </summary>
    public void ForceSetText(string newText)
    {
        // XRKeyboard 内部的 text 属性是 protected，
        // 在子类里可直接赋值并触发 onTextUpdated 事件。
        text = newText;
    }

    /// <summary>
    /// 重写 XRKeyboard.Open(...)：
    /// 如果想在键盘真正打开前，先把目标 InputField 的文本同步到键盘，可在这里处理。
    /// （也可以改在 XRKeyboardDisplay 的 OnInputFieldGainedFocus 中调用 ForceSetText）
    /// </summary>
    public override void Open(TMP_InputField inputField, bool observeCharacterLimit = false)
    {
        // 如果你希望在调用 base.Open(...) 之前，就将 inputField 的文本给键盘
        if (inputField != null)
        {
            ForceSetText(inputField.text);
        }

        // 调用基类逻辑
        base.Open(inputField, observeCharacterLimit);
    }
}
