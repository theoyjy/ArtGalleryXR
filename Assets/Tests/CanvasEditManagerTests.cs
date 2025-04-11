using Unity.Netcode;
using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;


public class CanvasEditManagerTests
{
    private GameObject managerGO;
    private CanvasEditManager manager;
    private GameObject editUI;
    private GameObject toolUI;
    private MockCard card;
    private GameObject player;
    private Button editButton;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        editUI = new GameObject("EditUI");
        var canvas = editUI.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace; // Required for world-space canvas
        editUI.SetActive(false); // Start inactive
        

        // Create and add a Button to the EditUI
        editButton = new GameObject("EditButton").AddComponent<Button>();
        editButton.transform.SetParent(editUI.transform);

        // Create mock Card object
       
        //cardObj.AddComponent<Card>(); // assumes you have a Card class
        //cardObj.tag = "Card";

        // Create CanvasEditManager GameObject
        managerGO = new GameObject("CanvasEditManager");
        manager = managerGO.AddComponent<CanvasEditManager>();

        card = managerGO.AddComponent<MockCard>();
        manager.card = card;
        //manager.card = cardObj.GetComponent<Card>();

        // Create UI GameObjects

        manager.EditUI = editUI;

        toolUI = new GameObject("ToolUI", typeof(Canvas));
        toolUI.SetActive(false);
        manager.ToolUI = toolUI;

        // Create local player with camera
        player = new GameObject("Player");
        player.tag = "Player";

        var netObj = player.AddComponent<NetworkObject>();

        var netManagerGO = new GameObject("NetManager");
        var netManager = netManagerGO.AddComponent<NetworkManager>();
        netManager.NetworkConfig = new NetworkConfig
        {
            NetworkTransport = netManagerGO.AddComponent<Unity.Netcode.Transports.UTP.UnityTransport>()
        };
        netManagerGO.SetActive(true);
        netManager.StartHost();

        yield return null; // wait one frame

        //netObj.SpawnWithOwnership(NetworkManager.Singleton.LocalClientId, true);
        GameObject cameraGO = new GameObject("Camera");
        Camera cameraComponent = cameraGO.AddComponent<Camera>();  // Add Camera component
        cameraComponent.orthographic = false; // Set it to perspective

        // Set the cameraGO as a child of the netObj (so it's part of the networked object)
        

        var parentGO = new GameObject("Parent"); // This represents the parent of the camera
        cameraGO.transform.SetParent(parentGO.transform);
        parentGO.transform.SetParent(player.transform); // Set camera as a child of parentGO

        // Create another empty GameObject to match the second level parent
        
        // Add ObjectMovementWithCamera to the grandparent (or the desired GameObject in the hierarchy)
        var objectMovement = parentGO.AddComponent<ObjectMovementWithCamera>();

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

        Assert.IsTrue(manager.ToolUI.activeSelf, "ToolUI Should be Active");

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
        player.AddComponent<ObjectMovementWithCamera>();
        camera.orthographic = true;
        manager.ToolUI.SetActive(true);

        manager.ExitEditMode();

        yield return null;

        Assert.IsFalse(manager.ToolUI.activeSelf, "ToolUI should be disabled after exiting edit mode.");
        Assert.IsFalse(camera.orthographic, "Camera should no longer be in orthographic mode.");
    }
}

public class MockCard : Card
{
    public override Vector3 GetNormal()
    {
        return Vector3.zero; 
    }
    
    public override Vector3 GetWorldLoc()
    {
        return Vector3.zero;
    }
    public override Quaternion GetWorldQuatRot()
    {
        // Return a fixed rotation (e.g., identity rotation)
        return Quaternion.Euler(0, 90, 0);  // Example: 90 degrees rotation around the Y-axis
    }
    

}