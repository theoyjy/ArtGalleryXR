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
        texture = new Texture2D((int)textureSize.x, (int)textureSize.y);
        whiteboardRenderer.material.mainTexture = texture;
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

    // Test: material changing function for eg
}
