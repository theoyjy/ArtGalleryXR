using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SFB;

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

    private void Start()
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

    private void Awake()
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
        if (Input.GetKeyDown(KeyCode.O))
        {
            SaveTextureToPNG();
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            LoadImageFromFile();
        }
    }

    public void ClearWhiteboard()
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

    void SaveTextureToPNG()
    {
        if (texture == null)
        {
            Debug.LogError("No texture assigned!");
            return;
        }

        // Create a new texture and set the rotated pixel data.
        Texture2D rotatedTexture = HandleFlip(texture);


        // Encode texture into PNG format
        byte[] bytes = rotatedTexture.EncodeToPNG();
#if !SERVER_BUILD && !UNITY_ANDROID && !UNITY_STANDALONE_OSX
        // Write to file
        StandaloneFileBrowserWindows windows = new StandaloneFileBrowserWindows();
        Debug.Log("projectPath: " + projectPath);
        string selectedSavePath = windows.SaveFilePanel("Save whiteboard image", projectPath, "Canvas.png", extensionFilters);
        if (selectedSavePath != null)
        {
            File.WriteAllBytes(selectedSavePath, bytes);
            Debug.Log($"Saved whiteboard image to: {selectedSavePath}");
        }
#endif
    }



    void LoadImageFromFile()
    {
#if !SERVER_BUILD && !UNITY_ANDROID && !UNITY_STANDALONE_OSX
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

        string filePath = paths[0];
        if (!File.Exists(filePath))
        {
            Debug.LogError($"File not found: {filePath}");
            return;
        }

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
#endif

    }

    public void ApplyTexture(Texture2D newTexture)
    {
        texture = new Texture2D(newTexture.width, newTexture.height, newTexture.format, false);
        texture.SetPixels(newTexture.GetPixels());
        texture.Apply();

        // Assign the texture to the material or renderer
        GetComponent<Renderer>().material.mainTexture = texture;
    }

    public Texture2D HandleFlip(Texture2D texture)
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

    public Texture2D ResizeTexture(Texture2D source, int newWidth, int newHeight)
    {
        // Create a temporary RenderTexture with the desired dimensions.
        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        RenderTexture.active = rt;

        // Copy (blit) the source texture to the RenderTexture.
        Graphics.Blit(source, rt);

        // Create a new Texture2D to hold the resized image.
        Texture2D resizedTex = new Texture2D(newWidth, newHeight, source.format, false);
        // Read the pixel data from the RenderTexture.
        resizedTex.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        resizedTex.Apply();

        // Cleanup
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        return resizedTex;
    }
}
