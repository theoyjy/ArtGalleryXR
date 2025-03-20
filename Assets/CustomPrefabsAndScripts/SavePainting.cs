using UnityEngine;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif
using SFB; // StandaloneFileBrowser (Install from GitHub)

public class SavePainting : MonoBehaviour
{
    public MeshRenderer paintingRenderer; // Drag & Drop the painting's MeshRenderer

    public void SaveImage()
    {
        if (paintingRenderer == null)
        {
            Debug.LogError("Painting Renderer is not assigned!");
            return;
        }

        // Get the texture from the material
        Texture2D paintingTexture = GetReadableTexture();
        if (paintingTexture == null)
        {
            Debug.LogError("Failed to create a readable texture!");
            return;
        }

        // Open Save File Dialog
        string path = GetSaveFilePath();
        if (!string.IsNullOrEmpty(path))
        {
            // Convert to PNG and Save
            File.WriteAllBytes(path, paintingTexture.EncodeToPNG());
            Debug.Log("Painting saved to: " + path);
        }
    }

    private Texture2D GetReadableTexture()
    {
        // Get the original texture
        Texture originalTexture = paintingRenderer.material.mainTexture;
        if (!(originalTexture is Texture2D sourceTexture))
        {
            return null;
        }

        // Create a new readable Texture2D
        RenderTexture tempRT = RenderTexture.GetTemporary(sourceTexture.width, sourceTexture.height, 0, RenderTextureFormat.ARGB32);
        Graphics.Blit(sourceTexture, tempRT);

        Texture2D readableTexture = new Texture2D(sourceTexture.width, sourceTexture.height, TextureFormat.RGBA32, false);
        RenderTexture.active = tempRT;
        readableTexture.ReadPixels(new Rect(0, 0, tempRT.width, tempRT.height), 0, 0);
        readableTexture.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(tempRT);

        return readableTexture;
    }

    private string GetSaveFilePath()
    {
#if UNITY_EDITOR
        return EditorUtility.SaveFilePanel("Save Painting", "", "painting.png", "png");
#else
        var extensions = new[] { new ExtensionFilter("PNG Files", "png") };
        string[] paths = StandaloneFileBrowser.SaveFilePanel("Save Painting", "", "painting", extensions);
        return paths.Length > 0 ? paths[0] : null;
#endif
    }
}