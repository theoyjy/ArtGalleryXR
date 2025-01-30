using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class MaterialConfigElement
{
    // 为每个 Renderer 配置多个材质
    public List<Renderer> renderers; // 需要更改材质的多个 Renderer
    public List<Material> rendererMaterials; // 每个 Renderer 对应一个材质列表

}

[System.Serializable]
public class MaterialConfig
{
    // 为每个 Renderer 配置多个材质
    public List<MaterialConfigElement> OneRenaderAndTheirMats; // 需要更改材质的多个 Renderer

}


public class MaterialSwitcher : MonoBehaviour
{
    // 存储多个配置，每个配置包含多个 Renderer 和对应的多个材质
    [SerializeField]
    private List<MaterialConfig> materialConfigs = new List<MaterialConfig>();

    // 当前的配置索引
    [SerializeField]
    private int currentConfigIndex = 0;

    private void OnValidate()
    {
        if (materialConfigs == null || materialConfigs.Count == 0)
        {
            return;  // 如果没有配置则返回
        }

        currentConfigIndex = Mathf.Clamp(currentConfigIndex, 0, materialConfigs.Count - 1);

        MaterialConfig currentConfig = materialConfigs[currentConfigIndex];

        // 遍历当前配置中的每个 MaterialConfigElement
        foreach (var configElement in currentConfig.OneRenaderAndTheirMats)
        {

            // 为每个 Renderer 更新材质
            for (int i = 0; i < configElement.renderers.Count; i++)
            {
                Renderer renderer = configElement.renderers[i];
                Material material = configElement.rendererMaterials[i];

                if (renderer != null && material != null)
                {
                    // 如果 Renderer 有多个材质，更新该 Renderer 的材质列表
                    if (renderer.sharedMaterials.Length > i)
                    {
                        renderer.sharedMaterials[i] = material;
                    }
                    else
                    {
                        Debug.LogWarning($"Renderer {renderer.name} does not have enough materials.");
                    }
                }
            }
        }

        Debug.Log("Switched to configuration: " + currentConfigIndex);
    }
}