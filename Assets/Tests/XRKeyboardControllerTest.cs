using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class XRKeyboardControllerTest
{
    private GameObject testObject;

    [SetUp]
    public void SetUp()
    {
        testObject = new GameObject();
        testObject.AddComponent<XRKeyboardController>();
    }

    [TearDown]
    public void TearDown()
    {
        Object.DestroyImmediate(testObject);
    }

    [Test]
    public void XRKeyboardController_ActiveState_IsCorrectPerPlatform()
    {
#if UNITY_ANDROID
        Assert.IsTrue(testObject.activeSelf, "GameObject should be active on Android.");
#else
        Assert.IsFalse(testObject.activeSelf, "GameObject should be inactive on non-Android platforms.");
#endif
    }
}
