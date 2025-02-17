using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
//using Mattatz.ColorPicker; // unity-color-picker 命名空间

public class ColorPickerUI : MonoBehaviour
{
    public Button openColorPickerButton;  // 触发颜色选择的按钮
    public GameObject colorPickerPanel;   // ColorPicker UI
    //public ColorPicker colorPicker;       // ColorPicker 组件
    public Button confirmButton;          // 确认按钮
    public Image selectedColorPreview;    // 预览颜色

    private Color selectedColor = Color.white;

    void Start()
    {
        // 默认隐藏颜色选择 UI
        colorPickerPanel.SetActive(false);
        confirmButton.gameObject.SetActive(false);

        // 监听按钮点击事件
        openColorPickerButton.onClick.AddListener(() => colorPickerPanel.SetActive(true));
        openColorPickerButton.onClick.AddListener(() => confirmButton.gameObject.SetActive(true));
        confirmButton.onClick.AddListener(ApplySelectedColor);

        // 监听颜色变化
        //colorPicker.onValueChanged += UpdateSelectedColor;
    }

    void UpdateSelectedColor(Color color)
    {
        selectedColor = color;
        selectedColorPreview.color = color; // 预览颜色
    }

    void ApplySelectedColor()
    {
        colorPickerPanel.SetActive(false);
        confirmButton.gameObject.SetActive(false);
        openColorPickerButton.image.color = selectedColor; // 更新按钮颜色
    }
}
