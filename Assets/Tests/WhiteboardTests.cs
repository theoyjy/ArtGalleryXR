using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TestTools;

public class WhiteboardTests
{
    [Test]
    public void TestInitialization()
    {
        // Setup
        var go = new GameObject();
        go.AddComponent<MeshRenderer>();
        var whiteboard = go.AddComponent<Whiteboard>();
        whiteboard.textureSize = new Vector2(2, 2);

        // Act
        whiteboard.Start();

        // Assert
        Assert.IsNotNull(whiteboard.texture);
        Assert.AreEqual(2, whiteboard.texture.width);
        Assert.AreEqual(2, whiteboard.texture.height);

        var pixels = whiteboard.texture.GetPixels();
        foreach (var pixel in pixels)
        {
            Assert.AreEqual(Color.white, pixel);
        }

        // Cleanup
        Object.DestroyImmediate(go);
    }

    [Test]
    public void TestClearWhiteboard()
    {
        // Setup
        var go = new GameObject();
        go.AddComponent<MeshRenderer>();
        var whiteboard = go.AddComponent<Whiteboard>();
        whiteboard.textureSize = new Vector2(2, 2);
        whiteboard.Start();

        // Pre-clear state
        var colors = new Color[] { Color.red, Color.blue, Color.green, Color.yellow };
        whiteboard.texture.SetPixels(colors);
        whiteboard.texture.Apply();

        // Act
        whiteboard.ClearWhiteboard();

        // Assert
        var clearedPixels = whiteboard.texture.GetPixels();
        foreach (var pixel in clearedPixels)
        {
            Assert.AreEqual(Color.white, pixel);
        }

        // Cleanup
        Object.DestroyImmediate(go);
    }

    [Test]
    public void TestApplyTexture()
    {
        // Setup
        var go = new GameObject();
        go.AddComponent<MeshRenderer>();
        var whiteboard = go.AddComponent<Whiteboard>();

        // Initialize whiteboard properly
        whiteboard.textureSize = new Vector2(2, 2);
        whiteboard.Start(); // Force initialization

        // Create test texture with explicit colors
        var testTexture = new Texture2D(2, 2);
        var expectedColors = new Color[] {
        Color.red,
        Color.blue,
        Color.green,
        new Color(1f, 0.9215686f, 0.01568628f, 1f) // Exact Color.yellow values
    };
        testTexture.SetPixels(expectedColors);
        testTexture.Apply();

        // Act
        whiteboard.ApplyTexture(testTexture);

        // Get actual colors from whiteboard
        var actualTexture = whiteboard.GetComponent<Renderer>().material.mainTexture as Texture2D;
        var actualColors = actualTexture.GetPixels();

        // Assert with tolerance
        for (int i = 0; i < expectedColors.Length; i++)
        {
            Assert.AreEqual(
                expectedColors[i].r, actualColors[i].r, 0.0001f,
                $"Red mismatch at {i}"
            );
            Assert.AreEqual(
                expectedColors[i].g, actualColors[i].g, 0.0001f,
                $"Green mismatch at {i}"
            );
            Assert.AreEqual(
                expectedColors[i].b, actualColors[i].b, 0.0001f,
                $"Blue mismatch at {i}"
            );
        }

        // Cleanup
        Object.DestroyImmediate(go);
    }

    [Test]
    public void TestHandleFlip()
    {
        // Setup test environment
        var go = new GameObject();
        var whiteboard = go.AddComponent<Whiteboard>();

        // Create a 2x2 test texture
        var testTexture = new Texture2D(2, 2);
        var originalColors = new Color[] {
        Color.red,
        Color.blue,
        Color.green,
        Color.yellow // Use Unity's actual yellow definition
    };
        testTexture.SetPixels(originalColors);
        testTexture.Apply();

        // Act
        var flippedTexture = whiteboard.HandleFlip(testTexture);
        var flippedColors = flippedTexture.GetPixels();

        // Expected results after 180° flip
        var expectedColors = new Color[] {
        Color.yellow,  // Original position (1,1)
        Color.green,   // Original position (0,1)
        Color.blue,    // Original position (1,0)
        Color.red      // Original position (0,0)
    };

        // Assert with color tolerance
        for (int i = 0; i < expectedColors.Length; i++)
        {
            Assert.AreEqual(
                expectedColors[i].r, flippedColors[i].r, 0.001f,
                $"Red channel mismatch at index {i}"
            );
            Assert.AreEqual(
                expectedColors[i].g, flippedColors[i].g, 0.001f,
                $"Green channel mismatch at index {i}"
            );
            Assert.AreEqual(
                expectedColors[i].b, flippedColors[i].b, 0.001f,
                $"Blue channel mismatch at index {i}"
            );
        }

        // Cleanup
        Object.DestroyImmediate(go);
    }

    [UnityTest]
    public IEnumerator TestResizeTexture()
    {
        // Setup - create required components
        var go = new GameObject();
        var renderer = go.AddComponent<MeshRenderer>();
        renderer.material = new Material(Shader.Find("Standard"));

        var whiteboard = go.AddComponent<Whiteboard>();
        whiteboard.textureSize = new Vector2(2, 2);
        whiteboard.whiteboardRenderer = renderer;

        // Initialize whiteboard properly
        whiteboard.Start();

        // Create source texture
        var source = new Texture2D(2, 2);
        source.SetPixels(new Color[] { Color.red, Color.blue, Color.green, Color.white });
        source.Apply();

        // Act
        var resized = whiteboard.ResizeTexture(source, 4, 4);
        yield return null; // Allow texture operations to complete

        // Assert
        Assert.AreEqual(4, resized.width);
        Assert.AreEqual(4, resized.height);

        // Check if original renderer texture remains unchanged
        Assert.AreNotEqual(resized, whiteboard.whiteboardRenderer.material.mainTexture);

        // Cleanup
        Object.DestroyImmediate(go);
        Object.DestroyImmediate(resized);
    }
}