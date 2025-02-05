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

        Vector3 CardNormal = card.GetNormal();
        Vector3 TargetCameraPosition = card.GetWorldLoc() + CardNormal * ToolSpawnOffset;

        GameObject tempTarget = new GameObject("TempTargetTransform");
        Transform targetTransform = tempTarget.transform;

        targetTransform.position = TargetCameraPosition;
        targetTransform.rotation = Quaternion.LookRotation(-CardNormal);
        Debug.Log("Target position: " + targetTransform.position + " Rotation: " + targetTransform.rotation);

        // hide enter edit canvas UI
        EditCanvasUI.SetActive(false);

        // move camera to focus on the canvas
        TeleportToTarget(targetTransform);


        // calculate FOV
        //float halfHeight = canvasSize.y / 2.0f;
        //float fovRadians = Mathf.Atan(halfHeight / distanceToCanvas);
        //float fovDegrees = fovRadians * Mathf.Rad2Deg * 2.0f;

        //cam.fieldOfView = fovDegrees;

        //// Optional: Match the camera's aspect ratio to the canvas
        //cam.aspect = canvasSize.x / canvasSize.y;

        // disable player movement

        //GetComponent<PlayerInput>().enabled = false;
        //GetComponent<ActionBasedContinuousMoveProvider>().enabled = false;
        //GetComponent<ActionBasedContinuousTurnProvider>().enabled = false;

        //StartCoroutine(ZoomCamera(TargetCameraPosition, CardNormal, zoomedInFOV));
        Instantiate(ToolUI);

    }

    public void ExitEditMode()
    {
        Debug.Log("Exiting edit mode");
        isEditMode = false;
        // Zoom out

        // Disable painting

        // Other exit logic
        //GetComponent<PlayerInput>().enabled = true;

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

    //IEnumerator ZoomCamera(Vector3 targetPosition, Vector3 CanvasNormal, float targetFOV)
    //{
    //    //yield return null;
    //    if (playerCamera == null)
    //        playerCamera = Camera.main;

    //    Vector3 startPosition = playerCamera.transform.position;
    //    Quaternion startRotation = playerCamera.transform.rotation;
    //    float startFOV = playerCamera.fieldOfView;
    //    float time = 0;
    //    float zoomSpeed = 1.0f; // Add zoomSpeed variable
    //    Debug.Log("ZoomCamera: EditCanvasUI.active= " + editedCanvas.GetComponent<Canvas>().enabled);

    //    while (time < 1)
    //    {
    //        time += Time.deltaTime * zoomSpeed;
    //        playerCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, time);

    //        playerCamera.transform.rotation = Quaternion.Slerp(startRotation,
    //                                                        Quaternion.LookRotation(-CanvasNormal),
    //                                                        time);
    //        playerCamera.fieldOfView = Mathf.Lerp(startFOV, targetFOV, time);
    //        yield return null;
    //    }
    //}

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
