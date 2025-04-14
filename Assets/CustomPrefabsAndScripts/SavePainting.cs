using UnityEngine;
using System.IO;
using UnityEngine.XR.Interaction.Toolkit.AR;

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
            // Handle the texture flipping
            Texture2D rotatedTexture = HandleFlip(paintingTexture);
            byte[] bytes = rotatedTexture.EncodeToPNG();

            File.WriteAllBytes(path, bytes);
            Debug.Log("Published painting saved to: " + path);

            // Convert to PNG and Save
            //File.WriteAllBytes(path, paintingTexture.EncodeToPNG());
            //Debug.Log("Painting saved to: " + path);
#if UNITY_ANDROID
            RefreshAndroidGallery(path);
#endif
        }
    }
    public virtual Texture2D HandleFlip(Texture2D texture)
    {
        int width = texture.width;
        int height = texture.height;

        // Get all pixels from the texture texture.
        Color[] originalPixels = texture.GetPixels();
        Color[] rotatedPixels = new Color[originalPixels.Length];

        // Re-map each pixel to its 180?rotated position.
        // The pixel at (x, y) goes to (width - 1 - x, height - 1 - y).
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int originalIndex = y * width + x;
                int rotatedIndex = (height - 1 - y) * width + (width - 1 - x);
                rotatedPixels[rotatedIndex] = originalPixels[originalIndex];
            }
        }

        // Create a new texture and set the rotated pixel data.
        Texture2D rotatedTexture = new Texture2D(width, height, texture.format, false);
        rotatedTexture.SetPixels(rotatedPixels);
        rotatedTexture.Apply();
        return rotatedTexture;
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
#elif UNITY_ANDROID
        return Path.Combine("/storage/emulated/0/Download/", "painting.png"); // Saves to Downloads folder on Quest 2
#elif !SERVER_BUILD
        var extensions = new[] { new ExtensionFilter("PNG Files", "png") };
        string path = StandaloneFileBrowser.SaveFilePanel("Save Painting", "", "painting", extensions);
        //StandaloneFileBrowserWindows windows = new StandaloneFileBrowserWindows();
        //string[] paths = windows.OpenFilePanel("Select an image", "", extensions, false);
        return path;
#else
        return Application.persistentDataPath + "/painting.png";
#endif
    }

#if UNITY_ANDROID
    private void RefreshAndroidGallery(string path)
    {
        AndroidJavaClass player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = player.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaObject context = activity.Call<AndroidJavaObject>("getApplicationContext");

        AndroidJavaClass mediaScanner = new AndroidJavaClass("android.media.MediaScannerConnection");
        mediaScanner.CallStatic("scanFile", context, new string[] { path }, null, null);

        Debug.Log("Android gallery refreshed: " + path);
    }
#endif
}