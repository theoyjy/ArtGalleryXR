using NUnit.Framework;
using UnityEngine;

public class UploadImageTests
{
    private UploadImage uploader;

    [SetUp]
    public void Setup()
    {
        GameObject go = new GameObject("Uploader");
        uploader = go.AddComponent<UploadImage>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(uploader.gameObject);
    }

    [Test]
    public void CalculateMD5_ReturnsExpectedHash()
    {
        byte[] data = System.Text.Encoding.UTF8.GetBytes("hello world");
        string hash = InvokePrivateMD5(data);

        // Precomputed MD5 hash of "hello world"
        Assert.AreEqual("5eb63bbbe01eeed093cb22bb8f5acdc3", hash);
    }

    [Test]
    public void ExtractSignedUrl_ReturnsCorrectValue()
    {
        string json = "{\"signed_url\":\"https:\\/\\/example.com\\/upload\\/12345.png\"}";
        string url = InvokePrivateExtractSignedUrl(json);

        Assert.AreEqual("https://example.com/upload/12345.png", url);
    }

    [Test]
    public void ExtractContentLink_ReturnsExpectedUrl()
    {
        string json = "{ \"content_link\": \"https://cdn.unity.com/files/abcd.png\" }";
        string link = InvokePrivateExtractContentLink(json);

        Assert.AreEqual("https://cdn.unity.com/files/abcd.png", link);
    }

    // Helper: Call private method CalculateMD5
    private string InvokePrivateMD5(byte[] data)
    {
        var method = typeof(UploadImage).GetMethod("CalculateMD5", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (string)method.Invoke(uploader, new object[] { data });
    }

    // Helper: Call private method ExtractSignedUrl
    private string InvokePrivateExtractSignedUrl(string json)
    {
        var method = typeof(UploadImage).GetMethod("ExtractSignedUrl", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (string)method.Invoke(uploader, new object[] { json });
    }

    // Helper: Call private method ExtractContentLink
    private string InvokePrivateExtractContentLink(string json)
    {
        var method = typeof(UploadImage).GetMethod("ExtractContentLink", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (string)method.Invoke(uploader, new object[] { json });
    }
}
