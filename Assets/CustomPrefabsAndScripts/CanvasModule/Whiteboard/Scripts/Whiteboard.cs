using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SFB;
using System;
using UnityEditor;

public class Whiteboard : MonoBehaviour
{
    public Texture2D texture;
    public Vector2 textureSize = new Vector2(2048, 2048);

    // Determine the save path
    private string projectPath;
    //private string savePath;
    //private string saveFileName;
    public Renderer whiteboardRenderer;
    public TextureSyncManager textureSyncManager;
#if !SERVER_BUILD
    ExtensionFilter[] extensionFilters;
#endif

    public void Start()
    {
        projectPath = Application.dataPath; // Get root project folder
        var r = GetComponent<Renderer>();
        texture = new Texture2D((int)textureSize.x, (int)textureSize.y);    

        //// Create a small white texture (1x1) instead of modifying every pixel manually
        Texture2D whiteTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);

        //// Fill the texture with white
        Color[] whitePixels = new Color[(int)(textureSize.x * textureSize.y)];
        for (int i = 0; i < whitePixels.Length; i++)
        {
            whitePixels[i] = Color.white;
        }
        texture.SetPixels(whitePixels);
        texture.Apply(); // Apply the changes

        //// Assign the texture to the material
        r.material.mainTexture = texture;
#if !SERVER_BUILD
        ExtensionFilter extensionFilter = new ExtensionFilter("Image Files", "png", "jpg", "jpeg");
        extensionFilters = new ExtensionFilter[] { extensionFilter };
#endif
    }

    public void Awake()
    {
        if(!texture)
        {
            texture = new Texture2D((int)textureSize.x, (int)textureSize.y);
            Debug.Log("No texture assigned to whiteboard, creating a new one.");
        }

        if(textureSyncManager == null)
        {
#if !SERVER_BUILD
            ExtensionFilter extensionFilter = new ExtensionFilter("Image Files", "png", "jpg", "jpeg");
            extensionFilters = new ExtensionFilter[] { extensionFilter };
#endif
          }
    }

    void Update()
    {
    }

    public virtual void ClearWhiteboard()
    {
        var r = GetComponent<Renderer>();
        texture = new Texture2D((int)textureSize.x, (int)textureSize.y);

        //// Create a small white texture (1x1) instead of modifying every pixel manually
        Texture2D whiteTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);

        //// Fill the texture with white
        Color[] whitePixels = new Color[(int)(textureSize.x * textureSize.y)];
        for (int i = 0; i < whitePixels.Length; i++)
        {
            whitePixels[i] = Color.white;
        }
        texture.SetPixels(whitePixels);
        texture.Apply(); // Apply the changes

        //// Assign the texture to the material
        r.material.mainTexture = texture;
    }

    public void SaveImage()
    {
        // Open Save File Dialog
        string path = GetSaveFilePath();
        if (!string.IsNullOrEmpty(path))
        {
            // Handle the texture flipping
            Texture2D rotatedTexture = HandleFlip(texture);
            byte[] bytes = rotatedTexture.EncodeToPNG();
         
            File.WriteAllBytes(path, bytes);
            Debug.Log("Easel Painting saved to: " + path);
#if UNITY_ANDROID
            RefreshAndroidGallery(path);
#endif
        }
    }

    private string GetSaveFilePath()
    {
#if UNITY_EDITOR
        return EditorUtility.SaveFilePanel("Save Painting", "", "painting.png", "png");
#elif UNITY_ANDROID
        return Path.Combine("/storage/emulated/0/Download/", "painting.png"); // Saves to Downloads folder on Quest 2
#elif !SERVER_BUILD
        var extensions = new[] { new ExtensionFilter("PNG Files", "png") };
        //string[] paths = StandaloneFileBrowser.SaveFilePanel("Save Painting", "", "painting", extensions);
        StandaloneFileBrowserWindows windows = new StandaloneFileBrowserWindows();
        string path = windows.SaveFilePanel("Save Painting", "", "painting", extensions);
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


    public void LoadImageFromFile()
    {
        string filePath;

#if UNITY_ANDROID
        filePath = Path.Combine("/storage/emulated/0/Download/", "painting.png");
#elif !SERVER_BUILD
        StandaloneFileBrowserWindows windows = new StandaloneFileBrowserWindows();
        string[] paths = windows.OpenFilePanel("Select an image", "", extensionFilters, false);
        if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
        {
            Debug.Log("Selected image: " + paths[0]);
        }
        else
        {
            Debug.Log("No file selected.");
            return;
        }

        filePath = paths[0];
#endif
        if (!File.Exists(filePath))
        {
            Debug.LogError($"File not found: {filePath}");
            return;
        }

        try
        {
            byte[] fileData = File.ReadAllBytes(filePath);
            texture = new Texture2D(2048, 2048); // Ensure correct dimensions

            if (texture.LoadImage(fileData)) // Load image data into texture
            {
                // Create a new texture and set the rotated pixel data.
                texture = ResizeTexture(texture, (int)textureSize.x, (int)textureSize.y);
                texture = HandleFlip(texture);
                whiteboardRenderer.material.mainTexture = texture;

                // sync texture to server
                textureSyncManager.SendTextureToServer();
                Debug.Log($"Loaded whiteboard image from: {filePath}");
            }
            else
            {
                Debug.LogError("Failed to load image file.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to read file from: {filePath}\n{e}");
            return;
        }
    }

    public virtual void ApplyTexture(Texture2D newTexture)
    {
        texture = new Texture2D(newTexture.width, newTexture.height, newTexture.format, false);
        texture.SetPixels(newTexture.GetPixels());
        texture.Apply();

        // Assign the texture to the material or renderer
        GetComponent<Renderer>().material.mainTexture = texture;
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

    public virtual Texture2D ResizeTexture(Texture2D source, int newWidth, int newHeight)
    {
        Texture2D result = new Texture2D(newWidth, newHeight, source.format, false);
        float incX = 1.0f / (float)newWidth;
        float incY = 1.0f / (float)newHeight;

        for (int y = 0; y < newHeight; y++)
        {
            for (int x = 0; x < newWidth; x++)
            {
                // Use bilinear interpolation from the source texture
                Color newColor = source.GetPixelBilinear(x * incX, y * incY);
                result.SetPixel(x, y, newColor);
            }
        }
        result.Apply();
        return result;
    }

}
