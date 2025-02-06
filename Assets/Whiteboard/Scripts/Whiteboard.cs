using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Whiteboard : MonoBehaviour
{
    public Texture2D texture;
    public Vector2 textureSize = new Vector2(2048, 2048);

    private string saveFileName = "testImageSave.png";

    void Start()
    {
        var r = GetComponent<Renderer>();
        texture = new Texture2D((int)textureSize.x, (int)textureSize.y);
        r.material.mainTexture = texture;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            SaveTextureToPNG(saveFileName);
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

        // Determine the save path
        //string path = Path.Combine(Application.persistentDataPath, filename);
        string path = "D:\\TCD_Courses\\ASE\\ArtGalleryXR\\exports\\" + filename;

        // Write to file
        File.WriteAllBytes(path, bytes);

        Debug.Log($"Saved whiteboard image to: {path}");
    }

    // Test: material changing function for eg
}
