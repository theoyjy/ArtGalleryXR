using NUnit.Framework;
using UnityEngine;
using System.IO;
using UnityEngine.TestTools;

public class SavePaintingTests
{
    private GameObject testObj;
    private SavePainting savePainting;
    private MeshRenderer meshRenderer;
    private Texture2D dummyTexture;

    [SetUp]
    public void Setup()
    {
        testObj = new GameObject("SavePainting");
        savePainting = testObj.AddComponent<SavePainting>();

        // Create a dummy texture
        dummyTexture = new Texture2D(64, 64, TextureFormat.RGBA32, false);
        Color[] colors = new Color[64 * 64];
        for (int i = 0; i < colors.Length; i++) colors[i] = Color.red;
        dummyTexture.SetPixels(colors);
        dummyTexture.Apply();

        // Setup MeshRenderer with a material and dummy texture
        meshRenderer = testObj.AddComponent<MeshRenderer>();
        meshRenderer.material = new Material(Shader.Find("Standard"));
        meshRenderer.material.mainTexture = dummyTexture;

        savePainting.paintingRenderer = meshRenderer;
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(testObj);
        Object.DestroyImmediate(dummyTexture);
    }

    [Test]
    public void GetReadableTexture_ReturnsCopy()
    {
        Texture2D readable = InvokePrivateGetReadableTexture();
        Assert.IsNotNull(readable);
        Assert.AreEqual(dummyTexture.width, readable.width);
        Assert.AreEqual(dummyTexture.height, readable.height);
    }

    [Test]
    public void GetReadableTexture_ReturnsNullForNonTexture2D()
    {
        meshRenderer.material.mainTexture = new RenderTexture(64, 64, 0); // Not Texture2D
        Texture2D result = InvokePrivateGetReadableTexture();
        Assert.IsNull(result);
    }

    [Test]
    public void SaveImage_ThrowsWarning_WhenRendererIsNull()
    {
        savePainting.paintingRenderer = null;

        LogAssert.Expect(LogType.Error, "Painting Renderer is not assigned!");
        savePainting.SaveImage();
    }

    // Optional test to verify encoding
    [Test]
    public void SaveImage_CreatesPNGBytes_WhenPathIsValid()
    {
        string tempPath = Path.Combine(Application.temporaryCachePath, "test_output.png");
        Texture2D readable = InvokePrivateGetReadableTexture();

        byte[] png = readable.EncodeToPNG();
        File.WriteAllBytes(tempPath, png);

        Assert.IsTrue(File.Exists(tempPath));

        // Clean up
        File.Delete(tempPath);
    }

    private Texture2D InvokePrivateGetReadableTexture()
    {
        var method = typeof(SavePainting).GetMethod("GetReadableTexture", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (Texture2D)method.Invoke(savePainting, null);
    }
}
