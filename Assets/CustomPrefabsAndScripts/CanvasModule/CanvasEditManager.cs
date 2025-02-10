using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using Unity.XR.CoreUtils;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

public class CanvasEditManager : MonoBehaviour
{
    private Camera playerCamera;
    private float defaultFOV = 60.0f;
    private float zoomedInFOV = 30.0f;
    private float zoomDistance = 10.0f;
    public float ToolSpawnOffset = 100.0f;
    GameObject editedCanvas;
    GameObject enterEditCanvasUI;
    TeleportationProvider teleportationProvider;
    Transform cameraTransformBeforeEnter;
    float originAspect;
    int oriWidth, oriHeight;

    [Header("UI Prefab (Must be a World-Space Canvas)")]
    [SerializeField] private GameObject ToolUI;

    private bool isEditMode = false;

    public static CanvasEditManager Instance { get; private set; }

    void Start()
    {
        isEditMode = false;
        playerCamera = Camera.main;
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
        
        if (playerCamera == null)
            playerCamera = Camera.main;

        if (teleportationProvider == null)
        {
            teleportationProvider = Object.FindAnyObjectByType<TeleportationProvider>();
        };

    }

    void Update()
    {

    }

    public void EnterEditMode(GameObject EditCanvasUI)
    {
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
        // record the camera position before entering edit mode
        if (playerCamera == null)
            playerCamera = Camera.main;

        cameraTransformBeforeEnter = playerCamera.transform.parent.transform.parent.transform;

        // calculate the target position and rotation
        Vector3 CardNormal = card.GetNormal();
        Vector3 TargetCameraPosition = card.GetWorldLoc() + CardNormal * ToolSpawnOffset;
        GameObject tempTarget = new GameObject("TempTargetTransform");
        Transform targetTransform = tempTarget.transform;
        targetTransform.position = TargetCameraPosition;
        //targetTransform.rotation = Quaternion.Euler(6.33f, 0f, 0f);
        Debug.Log("Target position: " + targetTransform.position + " Rotation: " + targetTransform.rotation);

        playerCamera.orthographic = true;
        playerCamera.orthographicSize = 10f;  // magic number
        oriWidth = Screen.width;
        oriHeight = Screen.height;
        Screen.SetResolution(2048, 2048, false);
        playerCamera.aspect = (float)Screen.width / Screen.height;

        // hide enter edit canvas UI
        EditCanvasUI.SetActive(false);

        // move camera to focus on the canvas
        TeleportToTarget(targetTransform);

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

        var trackedPoseDriver = playerCamera.transform.parent.GetComponentsInChildren<UnityEngine.InputSystem.XR.TrackedPoseDriver>(true);

        Debug.Log("TrackedPoseDriver count: " + trackedPoseDriver.Length);
        for (int i = 0; i < trackedPoseDriver.Length; i++)
        {
            trackedPoseDriver[i].enabled = true;
        }
        playerCamera.transform.parent.transform.parent.GetComponent<ObjectMovementWithCamera>().enabled = true;
        playerCamera.aspect = (float)Screen.width / Screen.height;
        playerCamera.orthographic = false;
        Screen.SetResolution(oriWidth, oriHeight, false);
        TeleportToTarget(cameraTransformBeforeEnter);
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

    public void TeleportToTarget(Transform targetTransform)
    {
        if (teleportationProvider == null || targetTransform == null)
        {
            Debug.LogWarning("TeleportationProvider or Target is missing!");
            return;
        }

        // Create the teleport request
        TeleportRequest teleportRequest = new TeleportRequest
        {
            destinationPosition = targetTransform.position,
            destinationRotation = targetTransform.rotation,
            matchOrientation = MatchOrientation.TargetUpAndForward
        };

        // Queue the teleport request
        teleportationProvider.QueueTeleportRequest(teleportRequest);
    }


}
