using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MaterialConfigElement
{
    // 单个 Renderer
    public Renderer renderer; 
    public List<Material> rendererMaterials; // 该 Renderer 对应的材质列表
}

[System.Serializable]
public class MaterialConfig
{
    public List<MaterialConfigElement> OneRenaderAndTheirMats; // 需要更改材质的 Renderer 配置
}

public class MaterialSwitcher : MonoBehaviour
{
    [SerializeField]
    private List<MaterialConfig> materialConfigs = new List<MaterialConfig>();

    [SerializeField]
    private int currentConfigIndex = 0;

    private void OnValidate()
    {
        if (materialConfigs == null || materialConfigs.Count == 0)
        {
            Debug.LogWarning("MaterialConfigs is empty.");
            return;
        }

        currentConfigIndex = Mathf.Clamp(currentConfigIndex, 0, materialConfigs.Count - 1);
        MaterialConfig currentConfig = materialConfigs[currentConfigIndex];

        foreach (var configElement in currentConfig.OneRenaderAndTheirMats)
        {
            if (configElement.renderer != null && configElement.rendererMaterials != null && configElement.rendererMaterials.Count > 0)
            {
                configElement.renderer.materials = configElement.rendererMaterials.ToArray(); // 赋值给 Renderer
                Debug.Log($"Updated {configElement.renderer.name} materials successfully.");
            }
            else
            {
                Debug.LogError($"Renderer is null or materials list is empty.");
            }
        }

        Debug.Log($"Switched to configuration: {currentConfigIndex}");
    }
}