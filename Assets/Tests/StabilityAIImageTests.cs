using NUnit.Framework;
using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.TestTools;


public class MockTextureSyncManager : TextureSyncManager
{
    public bool SendCalled = false;
    public override void SendTextureToServer()
    {
        SendCalled = true;
    }
}


public class StabilityAIImageTests
{
    private GameObject go;
    private StabilityAIImage stabilityAI;
    private Whiteboard whiteboardMock;
    private TextureSyncManager syncMock;
    private Renderer targetRenderer;

    [SetUp]
    public void Setup()
    {
        go = new GameObject("StabilityAIImage");
        stabilityAI = go.AddComponent<StabilityAIImage>();

        // Add targetRenderer with material
        targetRenderer = go.AddComponent<MeshRenderer>();
        targetRenderer.material = new Material(Shader.Find("Standard"));
        stabilityAI.targetRenderer = targetRenderer;

        // Create and assign mocked Whiteboard
        whiteboardMock = go.AddComponent<MockWhiteboard>();
        stabilityAI.whiteboard = whiteboardMock;

        // Create and assign mocked TextureSyncManager
        syncMock = go.AddComponent<MockTextureSyncManager>();
        stabilityAI.textureSyncManager = syncMock;
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(go);
    }

    [Test]
    public void OnInputFieldSubmitted_WithEmptyInput_LogsError()
    {
        LogAssert.Expect(LogType.Error, "No Input text");
        stabilityAI.OnInputFieldSubmitted("");
    }

    [UnityTest]
    public IEnumerator DownloadImage_AppliesTextureCorrectly()
    {
        // Create a fake red texture and convert to PNG bytes
        Texture2D tex = new Texture2D(2, 2);
        tex.SetPixels(new[] { Color.red, Color.red, Color.red, Color.red });
        tex.Apply();
        byte[] imageData = tex.EncodeToPNG();

        yield return stabilityAI.StartCoroutine(stabilityAI.GetType()
            .GetMethod("DownloadImage", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(stabilityAI, new object[] { imageData }) as IEnumerator);

        Assert.IsNotNull(stabilityAI.targetRenderer.material.mainTexture);
        Assert.IsTrue(((MockTextureSyncManager)stabilityAI.textureSyncManager).SendCalled);
    }
}

