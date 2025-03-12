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
    public Renderer whiteboardRenderer;

    private void Start()
    {
        projectPath = Application.dataPath.Replace("/Assets", "/exports"); // Get root project folder
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

    private void Awake()
    {
        if(!texture)
        {
            texture = new Texture2D((int)textureSize.x, (int)textureSize.y);
            Debug.Log("No texture assigned to whiteboard, creating a new one.");
        }
    }

    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.O))
        //{
        //    SaveTextureToPNG(saveFileName);
        //}
        if (Input.GetKeyDown(KeyCode.I))
        {
            LoadImageFromFile("testImageRead.png");
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

    void SaveTextureToPNG(string filename)
    {
        if (texture == null)
        {
            Debug.LogError("No texture assigned!");
            return;
        }

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

        // Encode texture into PNG format
        byte[] bytes = rotatedTexture.EncodeToPNG();

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

            // Encode texture into PNG format
            texture = rotatedTexture;
            whiteboardRenderer.material.mainTexture = texture;
            Debug.Log($"Loaded whiteboard image from: {filePath}");
        }
        else
        {
            Debug.LogError("Failed to load image file.");
        }
    }
}
