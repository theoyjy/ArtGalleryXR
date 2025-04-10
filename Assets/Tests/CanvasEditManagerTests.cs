using Unity.Netcode;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;


public class CanvasEditManagerTests
{
    private GameObject managerGO;
    private CanvasEditManager manager;
    private GameObject editUI;
    private GameObject toolUI;
    private GameObject cardObj;
    private GameObject player;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // Create mock Card object
        cardObj = new GameObject("Card");
        cardObj.AddComponent<Card>(); // assumes you have a Card class
        cardObj.tag = "Card";

        // Create CanvasEditManager GameObject
        managerGO = new GameObject("CanvasEditManager");
        manager = managerGO.AddComponent<CanvasEditManager>();
        manager.card = cardObj.GetComponent<Card>();

        // Create UI GameObjects
        editUI = new GameObject("EditUI", typeof(Canvas));
        editUI.SetActive(false);
        manager.EditUI = editUI;

        toolUI = new GameObject("ToolUI", typeof(Canvas));
        toolUI.SetActive(false);
        manager.ToolUI = toolUI;

        // Create local player with camera
        player = new GameObject("Player");
        player.tag = "Player";
        var networkObj = player.AddComponent<NetworkObject>();
        networkObj.SpawnWithOwnership(NetworkManager.Singleton.LocalClientId, true);

        GameObject cameraGO = new GameObject("Camera", typeof(Camera));
        cameraGO.transform.SetParent(player.transform);
        cameraGO.GetComponent<Camera>().orthographic = false;

        yield return null;
    }

    [UnityTest]
    public IEnumerator EditUI_IsEnabled_OnTriggerEnter()
    {
        // Simulate entering trigger
        var collider = player.AddComponent<BoxCollider>();
        collider.isTrigger = true;

        manager.GetType().GetMethod("OnTriggerEnter", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .Invoke(manager, new object[] { collider });

        yield return null;

        Assert.IsTrue(manager.EditUI.activeSelf, "EditUI should be enabled after OnTriggerEnter.");
    }

    [UnityTest]
    public IEnumerator EnterEditMode_SetsOrthographicCamera_DesktopOnly()
    {
        var networkObj = player.GetComponent<NetworkObject>();
        var camera = player.GetComponentInChildren<Camera>();

        // Mock required component
        player.AddComponent<ObjectMovementWithCamera>();

        manager.EnterEditMode(editUI);

        yield return null;

#if !UNITY_ANDROID
        Assert.IsTrue(camera.orthographic, "Camera should be orthographic in desktop mode.");
#endif
    }

    [UnityTest]
    public IEnumerator ExitEditMode_DisablesToolUI_AndRestoresPerspective()
    {
        var networkObj = player.GetComponent<NetworkObject>();
        var camera = player.GetComponentInChildren<Camera>();

        camera.orthographic = true;
        manager.ToolUI.SetActive(true);

        manager.ExitEditMode();

        yield return null;

        Assert.IsFalse(manager.ToolUI.activeSelf, "ToolUI should be disabled after exiting edit mode.");
        Assert.IsFalse(camera.orthographic, "Camera should no longer be in orthographic mode.");
    }
}