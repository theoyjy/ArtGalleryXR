using UnityEngine;
using System.Collections;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Teleportation;
using Unity.XR.CoreUtils;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.Netcode;
using UnityEngine.UI;

public class CanvasEditManager : MonoBehaviour
{
    private Camera playerCamera;
    private float defaultFOV = 60.0f;
    private float zoomedInFOV = 30.0f;
    private float zoomDistance = 10.0f;
    public float ToolSpawnOffset = 100.0f;
    private Button editButton;
   
    Vector3 cameraBeforeEnterPosition;
    Quaternion cameraBeforeEnterRotation;
    NetworkObject localPlayer = null;
    float originAspect;

    [Header("UI Prefab (Must be a World-Space Canvas)")]
    [SerializeField] private GameObject ToolUI;

    [SerializeField] private GameObject EditUI;

    public Card card;

    private bool isEditMode = false;

    public static CanvasEditManager Instance { get; private set; }

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

    void Start()
    {
        isEditMode = false;
        cameraBeforeEnterPosition = Vector3.zero;
        cameraBeforeEnterRotation = Quaternion.identity;

        EditUI.SetActive(false);
        ToolUI.SetActive(false);
    }

    

    private void OnTriggerEnter(Collider other)
    {
        NetworkObject player = GetCurrentPlayer();
        if (player == null)
        {
            Debug.Log("No local player yet in the scene.");
            return;
        }
        Camera playerCamera = player.GetComponentInChildren<Camera>();

        if (other.CompareTag("Player"))
        {
            Camera cam = other.GetComponentInChildren<Camera>();
            if (playerCamera != cam)
            {
                Debug.Log("This is not the local player's camera");
                return;
            }
        }
        else
        {
            Debug.Log("Trigger entered with: " + other.tag);
            return;
        }


        EditUI.SetActive(true);
        Canvas canvas = EditUI.GetComponent<Canvas>();
        canvas.worldCamera = playerCamera;
        Debug.Log("Trigger Entered: Setting EditUI Active");
        editButton = EditUI.GetComponentInChildren<Button>();
        editButton.onClick.AddListener(() => EnterEditMode(gameObject));
    }

    private void OnTriggerExit(Collider other)
    {

        EditUI.SetActive(false);
        ToolUI.SetActive(false);
        isEditMode = false;
    }


    NetworkObject GetCurrentPlayer()
    {
        if(localPlayer == null)
        {
            foreach (var player in FindObjectsOfType<NetworkObject>())
            {
                if (player.CompareTag("Player") && player.IsOwner) // This is the local player
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

        isEditMode = true;

        Debug.Log("ToggleEditMode after " + isEditMode);

       
        if (card == null)
        {
            Debug.LogError("Card component is missing from: " + gameObject.name);
            return;
        }

        // record the camera position before entering edit mode
        cameraTransformBeforeEnter = playerCamera.transform.parent.transform.parent.transform;
        originAspect = playerCamera.aspect;

        NetworkObject player = GetCurrentPlayer();
        Camera playerCamera = player.GetComponentInChildren<Camera>();
        EditUI.SetActive(false);
#if UNITY_ANDROID

#else
        MoveCameraToOrthographic(player, playerCamera);
#endif  
        ToolUI.SetActive(true);
        Canvas canvas = ToolUI.GetComponent<Canvas>();
        canvas.worldCamera = playerCamera;
        Button[] buttons = ToolUI.GetComponentsInChildren<Button>();
        foreach (Button button in buttons)
        {
            if (button.name == "ExitBtn")
                button.onClick.AddListener(() => ExitEditMode());
            else if (button.name == "DeleteBtn")
                button.onClick.AddListener(() => DeleteCanvas());

        }
        playerCamera.transform.parent.transform.parent.GetComponent<ObjectMovementWithCamera>().enabled = false;
        Instantiate(ToolUI);

    }

    public void ExitEditMode()
    {
        Debug.Log("Exiting edit mode");
        isEditMode = false;
        ToolUI.SetActive(false);

#if UNITY_ANDROID

#else
        MoveCameraToPerspective();
#endif
    }

    public void DeleteCanvas()
    {
        Debug.Log("Deleting canvas");
        if (card == null)
        {
            Debug.LogError("Card component is missing from: " + gameObject.name);
            return;
        }
        card.DeleteDrawing();
    }

    public void TeleportToTarget(Transform targetTransform)
    {
        Debug.Log("Teleport to: " + CameraPosition + " rotation: " + CameraRotation);
        NetworkObject player = GetCurrentPlayer();
        player.transform.position = CameraPosition;
        player.transform.rotation = CameraRotation;
    }

    public void MoveCameraToOrthographic(NetworkObject player, Camera playerCamera)
    {
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
        

        // move camera to focus on the canvas
        MoveXRToTargetTrans(TargetCameraPosition, TargetCameraRotation);

        var trackedPoseDriver = playerCamera.transform.parent.GetComponentsInChildren<UnityEngine.InputSystem.XR.TrackedPoseDriver>(true);

        Debug.Log("TrackedPoseDriver count: " + trackedPoseDriver.Length);
        for (int i = 0; i < trackedPoseDriver.Length; i++)
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

    private void MoveCameraToPerspective()
    {
        NetworkObject player = GetCurrentPlayer();
        Camera playerCamera = player.GetComponentInChildren<Camera>();
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
        MoveXRToTargetTrans(cameraBeforeEnterPosition, cameraBeforeEnterRotation);
    }


}
