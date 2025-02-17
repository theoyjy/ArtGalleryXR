using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;
 // unity-color-picker 命名空间

public class ColorPickerUI : MonoBehaviour
{
    public Button openColorPickerButton;  // 触发颜色选择的按钮
    public GameObject colorPickerPanel;   // ColorPicker UI
    public FlexibleColorPicker colorPicker;       // ColorPicker 组件
    public Button confirmButton;          // 确认按钮
    public Image selectedColorPreview;    // 预览颜色

    public Color SendColor = Color.white;

    private Color selectedColor = Color.white;
    private MarkerColorChanger markerColorChanger;

    private GameObject canvasComponent;

    GameObject FindTopLevelGameObject(string objectName)
    {
        // 获取所有场景中最上层的根节点 GameObject
        GameObject[] rootObjects = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();

        // 遍历所有根 GameObject，查找目标名字
        foreach (GameObject obj in rootObjects)
        {
            if (obj.name == objectName)
            {
                return obj; // 立即返回，提高性能
            }
        }

        return null; // 没找到
    }

    void Start()
    {
        canvasComponent = FindTopLevelGameObject("CanvasComponent");

        if (canvasComponent != null)
        {
            Debug.Log("✅ 找到 CanvasComponent：" + canvasComponent.name);
            // 在 CanvasComponent 及其所有子节点查找 MarkerColorChanger 组件
            markerColorChanger = canvasComponent.GetComponentInChildren<MarkerColorChanger>();

            if (markerColorChanger != null)
            {
                Debug.Log("✅ 成功找到 MarkerColorChanger：" + markerColorChanger.gameObject.name);
                
            }
            else
            {
                Debug.LogWarning("⚠️ CanvasComponent 下面没有找到 MarkerColorChanger 组件！");
            }
        }
        else
        {
            Debug.LogError("❌ 场景中没有找到 CanvasComponent！");
        }

        // 默认隐藏颜色选择 UI
        colorPickerPanel.SetActive(false);
        confirmButton.gameObject.SetActive(false);
        colorPicker = colorPickerPanel.GetComponent<FlexibleColorPicker>();
        // 监听颜色变化
        colorPicker.onColorChange.AddListener(UpdateSelectedColor);

        // 监听按钮点击事件
        openColorPickerButton.onClick.AddListener(() => colorPickerPanel.SetActive(true));
        openColorPickerButton.onClick.AddListener(() => confirmButton.gameObject.SetActive(true));
        //confirm 点击调用
        confirmButton.onClick.AddListener(ApplySelectedColor);
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
        SendColor = selectedColor;
        colorPicker.color = selectedColor;
        if(markerColorChanger != null)
        {
            markerColorChanger.newColor = selectedColor;
        }
    }
}
