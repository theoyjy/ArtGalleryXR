using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Whiteboard : MonoBehaviour
{
    public Texture2D texture;
    public Vector2 textureSize = new Vector2(2048, 2048);

    // Determine the save path
    private string projectPath;
    private string savePath;
    private string saveFileName;
    private Renderer whiteboardRenderer;

    void Start()
    {
        projectPath = Application.dataPath.Replace("/Assets", "/exports/"); // Get root project folder
        saveFileName = "testImageSave.png";
        savePath = Path.Combine(projectPath, saveFileName);
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

    public void ClearWhiteboard()
    {
        if (texture == null)
        {
            Debug.LogWarning("Whiteboard texture is null.");
            return;
        }

        // Create an array filled with white pixels.
        Color[] whitePixels = new Color[texture.width * texture.height];
        for (int i = 0; i < whitePixels.Length; i++)
        {
            whitePixels[i] = Color.white;
        }

        // Apply the white color array to the texture and update it.
        texture.SetPixels(whitePixels);
        texture.Apply();

        Debug.Log("Whiteboard cleared.");
    }

    void Update()
    {
        if (PlayFabManager.IsLoginActive)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            SaveTextureToPNG(saveFileName);
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            LoadImageFromFile("testImageRead.png");
        }
    }

    void SaveTextureToPNG(string filename)
    {
        if (texture == null)
        {
            Debug.LogError("No texture assigned!");
            return;
        }

        // Encode texture into PNG format
        byte[] bytes = texture.EncodeToPNG();

        // Write to file
        File.WriteAllBytes(savePath, bytes);

        Debug.Log($"Saved whiteboard image to: {savePath}");
    }

    

    void LoadImageFromFile(string readFileName)
    {
        string filePath = Path.Combine(projectPath, readFileName);
        if (!File.Exists(filePath))
        {
            Debug.LogError($"File not found: {filePath}");
            return;
        }

        byte[] fileData = File.ReadAllBytes(filePath);
        texture = new Texture2D(2048, 2048); // Ensure correct dimensions

        if (texture.LoadImage(fileData)) // Load image data into texture
        {
            whiteboardRenderer.material.mainTexture = texture;
            Debug.Log($"Loaded whiteboard image from: {filePath}");
        }
        else
        {
            Debug.LogError("Failed to load image file.");
        }
    }
}
