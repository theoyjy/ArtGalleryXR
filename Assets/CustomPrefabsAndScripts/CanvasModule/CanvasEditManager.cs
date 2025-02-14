using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using Unity.XR.CoreUtils;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.Netcode;

public class CanvasEditManager : MonoBehaviour
{
    private float defaultFOV = 60.0f;
    private float zoomedInFOV = 30.0f;
    private float zoomDistance = 10.0f;
    public float ToolSpawnOffset = 100.0f;
    GameObject editedCanvas;
    GameObject enterEditCanvasUI;
    Vector3 cameraBeforeEnterPosition;
    Quaternion cameraBeforeEnterRotation;
    NetworkObject localPlayer = null;
    float originAspect;
    int oriWidth, oriHeight;

    [Header("UI Prefab (Must be a World-Space Canvas)")]
    [SerializeField] private GameObject ToolUI;

    private bool isEditMode = false;

    public static CanvasEditManager Instance { get; private set; }

    void Start()
    {
        isEditMode = false;
        cameraBeforeEnterPosition = Vector3.zero;
        cameraBeforeEnterRotation = Quaternion.identity;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // Not be destroyed when loading a new scene
        }
        else
        {
            Destroy(gameObject);  // Only one instance of this object is allowed
            return;
        }

    }
    NetworkObject GetCurrentPlayer()
    {
        if(localPlayer == null)
        {
            foreach (var player in FindObjectsOfType<NetworkObject>())
            {
                if (player.IsOwner) // This is the local player
                {
                    localPlayer = player;
                    break;
                }
            }
        }
        return localPlayer;
    }

    public void EnterEditMode(GameObject EditCanvasUI)
    {
        if (isEditMode)
        {
            Debug.Log("Already in Edit Mode");
            return;
        }

        enterEditCanvasUI = EditCanvasUI;
        EnterEditController UIController = EditCanvasUI.GetComponent<EnterEditController>();
        if (UIController == null)
        {
            Debug.LogError("UIController component is missing from: " + EditCanvasUI.name);
            return;
        }
        editedCanvas = UIController.Canvas;
        isEditMode = true;

        Debug.Log("ToggleEditMode after " + isEditMode);

        Card card = editedCanvas.GetComponentInChildren<Card>();
        if (card == null)
        {
            Debug.LogError("Card component is missing from: " + gameObject.name);
            return;
        }

#if UNITY_ANDROID

#else
        // record the camera position before entering edit mode

        NetworkObject player = GetCurrentPlayer();
        Camera playerCamera = player.GetComponentInChildren<Camera>();

        // struct value copy
        cameraBeforeEnterPosition = player.transform.position;
        cameraBeforeEnterRotation = player.transform.rotation;

        // calculate the target position and rotation
        Vector3 CardNormal = card.GetNormal();
        Vector3 TargetCameraPosition = card.GetWorldLoc() + CardNormal * ToolSpawnOffset;
        GameObject tempTarget = new GameObject("TempTargetTransform");

        Quaternion TargetCameraRotation = card.GetWorldQuatRot();
        Debug.Log("Target position: " + TargetCameraPosition + " Rotation: " + TargetCameraRotation);

        playerCamera.orthographic = true;
        playerCamera.orthographicSize = 10f;  // magic number
        oriWidth = Screen.width;
        oriHeight = Screen.height;
        Screen.SetResolution(2048, 2048, false);
        playerCamera.aspect = (float)Screen.width / Screen.height;

        // hide enter edit canvas UI
        EditCanvasUI.SetActive(false);

        // move camera to focus on the canvas
        MoveXRToTargetTrans(TargetCameraPosition, TargetCameraRotation);

        var trackedPoseDriver = playerCamera.transform.parent.GetComponentsInChildren<UnityEngine.InputSystem.XR.TrackedPoseDriver>(true);

        Debug.Log("TrackedPoseDriver count: " + trackedPoseDriver.Length);
        for (int i = 0; i < trackedPoseDriver.Length; i++)
        {
            trackedPoseDriver[i].enabled = false;
        }
        playerCamera.transform.parent.transform.parent.GetComponent<ObjectMovementWithCamera>().enabled = false;

#endif
        Instantiate(ToolUI);
    }

    public void ExitEditMode()
    {
        Debug.Log("Exiting edit mode");
        isEditMode = false;
#if UNITY_ANDROID

#else
        NetworkObject player = GetCurrentPlayer();
        Camera playerCamera = player.GetComponentInChildren<Camera>();
        var trackedPoseDriver = playerCamera.transform.parent.GetComponentsInChildren<UnityEngine.InputSystem.XR.TrackedPoseDriver>(true);
        enterEditCanvasUI.SetActive(true);

        Debug.Log("TrackedPoseDriver count: " + trackedPoseDriver.Length);
        for (int i = 0; i < trackedPoseDriver.Length; i++)
        {
            trackedPoseDriver[i].enabled = true;
        }
        playerCamera.transform.parent.transform.parent.GetComponent<ObjectMovementWithCamera>().enabled = true;
        playerCamera.aspect = (float)Screen.width / Screen.height;
        playerCamera.orthographic = false;
        Screen.SetResolution(oriWidth, oriHeight, false);
        MoveXRToTargetTrans(cameraBeforeEnterPosition, cameraBeforeEnterRotation);
#endif
    }

    public void DeleteCanvas()
    {
        Debug.Log("Deleting canvas");
        Card card = editedCanvas.GetComponentInChildren<Card>();
        if (card == null)
        {
            Debug.LogError("Card component is missing from: " + gameObject.name);
            return;
        }
        card.DeleteDrawing();
    }

    public void MoveXRToTargetTrans(Vector3 CameraPosition, Quaternion CameraRotation)
    {
        Debug.Log("Teleport to: " + CameraPosition + " rotation: " + CameraRotation);
        NetworkObject player = GetCurrentPlayer();
        player.transform.position = CameraPosition;
        player.transform.rotation = CameraRotation;
    }


}
