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
        whiteboardRenderer = GetComponent<Renderer>();

        var r = GetComponent<Renderer>();       

        // Create a small white texture (1x1) instead of modifying every pixel manually
        Texture2D whiteTex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        whiteTex.SetPixel(0, 0, Color.white);
        whiteTex.Apply();

        // Create a new blank 2048x2048 texture
        texture = new Texture2D((int)textureSize.x, (int)textureSize.y, TextureFormat.RGBA32, false);

        // Use Graphics.Blit() to efficiently fill the texture using GPU
        RenderTexture rt = new RenderTexture(texture.width, texture.height, 0);
        Graphics.Blit(whiteTex, rt);

        // Copy RenderTexture to Texture2D
        RenderTexture.active = rt;
        texture.ReadPixels(new Rect(0, 0, texture.width, texture.height), 0, 0);
        texture.Apply();
        RenderTexture.active = null;
        rt.Release();

        // Assign the texture to the material
        r.material.mainTexture = texture;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
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
