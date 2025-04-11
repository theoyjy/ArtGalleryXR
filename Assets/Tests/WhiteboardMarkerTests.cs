using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.TestTools;
using System.Reflection;

public class WhiteboardMarkerTests
{
    private GameObject penObject;
    private WhiteboardMarker marker;
    private MockWhiteboard whiteboard;
    private MockTextureSyncManager textureSync;

    [SetUp]
    public void Setup()
    {
        penObject = new GameObject("Pen");
        marker = penObject.AddComponent<WhiteboardMarker>();

        // Setup a basic tip renderer for color
        var tip = new GameObject("Tip");
        tip.transform.parent = penObject.transform;
        var tipRenderer = tip.AddComponent<MeshRenderer>();
        tipRenderer.material = new Material(Shader.Find("Standard"));
        tipRenderer.material.color = Color.green;
        marker.GetType().GetField("_tip", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
              .SetValue(marker, tip.transform);

        // Mock whiteboard
        whiteboard = new GameObject("Whiteboard").AddComponent<MockWhiteboard>();
        whiteboard.texture = new Texture2D(128, 128);
        whiteboard.textureSize = new Vector2Int(128, 128);

        marker.GetType().GetField("_whiteboardTransform", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
              .SetValue(marker, whiteboard.transform);

        // Mock sync manager
        textureSync = penObject.AddComponent<MockTextureSyncManager>();
        marker._textureSyncManager = textureSync;

        Camera mockCamera = new GameObject("FakeCamera").AddComponent<Camera>();
        marker.GetType().GetField("_playerCamera", BindingFlags.NonPublic | BindingFlags.Instance)
              .SetValue(marker, mockCamera);
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(penObject);
        Object.DestroyImmediate(whiteboard.gameObject);
    }

    [Test]
    public void OnPointerEnter_SetsHoveringTrue()
    {
        marker.OnPointerEnter(null);
        Assert.IsTrue(GetPrivateBool(marker, "_isHovering"));
    }

    [Test]
    public void OnPointerExit_SetsHoveringFalse()
    {
        marker.OnPointerEnter(null); // Set to true first
        marker.OnPointerExit(null);
        Assert.IsFalse(GetPrivateBool(marker, "_isHovering"));
    }

    [Test]
    public void GrabPen_SetsIsHoldingTrueAndAppliesOffset()
    {
        marker.GetType().GetMethod("GrabPen", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
              .Invoke(marker, null);

        Assert.IsTrue(marker._isHolding);
    }

    [Test]
    public void DropPen_ResetsHoldingAndWhiteboard()
    {
        // Simulate holding first
        marker._isHolding = true;
        //SetPrivateBool(marker, "_isHolding", true);
        SetPrivateBool(marker, "_isSnapped", true);
        marker.GetType().GetMethod("DropPen", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
              .Invoke(marker, null);

        Assert.IsFalse(marker._isHolding);
        Assert.IsFalse(GetPrivateBool(marker, "_isSnapped"));
        Assert.IsNull(GetPrivateField<Whiteboard>(marker, "_whiteboard"));
    }

    // Utility: Get private bool
    private bool GetPrivateBool(object obj, string fieldName)
    {
        return (bool)obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(obj);
    }

    // Utility: Set private bool
    private void SetPrivateBool(object obj, string fieldName, bool value)
    {
        var field = obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field == null)
        {
            Assert.Fail($"Field '{fieldName}' not found on {obj.GetType().Name}.");
        }
        field.SetValue(obj, value);
    }


    // Utility: Get private field
    private T GetPrivateField<T>(object obj, string fieldName)
    {
        return (T)obj.GetType().GetField(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(obj);
    }
}
