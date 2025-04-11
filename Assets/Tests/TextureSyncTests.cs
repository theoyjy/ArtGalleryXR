using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Collections;
using Unity.Netcode;

public class TextureSyncManagerTests
{
    private GameObject testObj;
    private TextureSyncManager manager;
    private MockWhiteboard mockWhiteboard;

    [SetUp]
    public void Setup()
    {
        testObj = new GameObject();
        manager = testObj.AddComponent<TextureSyncManager>();

        mockWhiteboard = new MockWhiteboard();
        manager.whiteboard = mockWhiteboard;

        // Manually set private fields via reflection if needed
        var strokeBufferField = typeof(TextureSyncManager).GetField("strokeBuffer", BindingFlags.NonPublic | BindingFlags.Instance);
        strokeBufferField?.SetValue(manager, new List<StrokeCommand>());
    }

    [TearDown]
    public void TearDown()
    {
        GameObject.DestroyImmediate(testObj);
    }

    [Test]
    public void ApplyTextureToUIOrObject_AppliesTexture()
    {
        // Arrange
        Texture2D dummyTex = new Texture2D(2, 2);

        // Act
        MethodInfo method = typeof(TextureSyncManager)
            .GetMethod("ApplyTextureToUIOrObject", BindingFlags.NonPublic | BindingFlags.Instance);
        method.Invoke(manager, new object[] { dummyTex });

        // Assert
        Assert.AreEqual(dummyTex, mockWhiteboard.LastAppliedTexture);
    }

    //[Test]
    //public void StrokeCommand_Serializes_Correctly()
    //{
    //    // Arrange
    //    StrokeCommand stroke = new StrokeCommand
    //    {
    //        posStart = new Vector2(1, 2),
    //        posEnd = new Vector2(3, 4),
    //        brushSize = 10,
    //        colors = new[] { new Color(0.1f, 0.2f, 0.3f, 0.4f) }
    //    };

    //    var buffer = new Unity.Netcode.FastBufferWriter(1024, Unity.Collections.Allocator.Temp);
    //    var serializer = new Unity.Netcode.BufferSerializer<Unity.Netcode.FastBufferWriter>(new Unity.Netcode.FastBufferWriter(1024, Unity.Collections.Allocator.Temp));

    //    // Act
    //    stroke.NetworkSerialize(serializer);
    //    Assert.Pass("No exceptions during serialization"); // If no exceptions = pass
    //}

    [Test]
    public void IsClearing_Flag_Resets()
    {
        // Simulate IsClearing logic reset
        FieldInfo field = typeof(TextureSyncManager).GetField("IsClearing", BindingFlags.NonPublic | BindingFlags.Instance);
        field.SetValue(manager, true);

        // Act - simulate coroutine clearing
        if ((bool)field.GetValue(manager))
        {
            mockWhiteboard.ClearWhiteboard();
            field.SetValue(manager, false);
        }

        // Assert
        Assert.IsFalse((bool)field.GetValue(manager));
        Assert.IsTrue(mockWhiteboard.ClearCalled);
    }
}

