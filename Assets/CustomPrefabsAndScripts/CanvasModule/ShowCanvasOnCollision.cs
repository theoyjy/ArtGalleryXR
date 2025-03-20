using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;


public class ShowCanvasOnCollision : MonoBehaviour
{
    [Header("UI Prefab (Must be a World-Space Canvas)")]
    [SerializeField] private GameObject EditUI;

    [Header("Tag to Detect (e.g., 'Interactable')")]
    [SerializeField] private string interactableTag = "Player";

    [Header("Spawn Offset from Collision (Optional)")]
    [SerializeField] public float spawnOffset = 0.1f;


    [SerializeField] GameObject ToolUI;
    private Dictionary<Collider, GameObject> activeUIs = new Dictionary<Collider, GameObject>();


    // If using a networking framework that distinguishes local players:
    // private bool isLocalPlayer = true; // Or if using Mirror: [SyncVar] public bool isLocalPlayer;

    private void OnTriggerEnter(Collider other)
    {


        Debug.Log(" Trigger entered with: " + other.tag);
        Camera localCamera = Camera.main;
        if (other.CompareTag(interactableTag))
        {
            Camera cam = other.GetComponentInChildren<Camera>();
            foreach (var player in FindObjectsOfType<NetworkObject>())
            {
                if (player.IsOwner) // This is the local player
                {
                    localCamera = player.GetComponentInChildren<Camera>();
                    if (localCamera != cam)
                    {
                        Debug.Log("This is not the local player's camera");
                        return;
                    }
                }
            }



            Card card = GetComponentInChildren<Card>();
            if (card == null)
            {
                Debug.LogError("Card component is missing from: " + gameObject.name);
                return;
            }
            
            if (activeUIs.ContainsKey(other))
            {
                Debug.Log("UI already exists for " + other.name + " so destroy previous");
                Destroy(activeUIs[other]);
            }

            Vector3 spawnPosition = card.GetWorldLoc() + spawnOffset * card.GetNormal();
            Debug.Log("Spawn position: " + spawnPosition);

//#if UNITY_ANDROID



            //Canvas canvas = ToolUI.GetComponent<Canvas>();
            //canvas.enabled = true;
            //if (canvas.worldCamera == null){
            //    canvas.worldCamera = localCamera;
            //}

            //EnterEditController uiController = EditUI.GetComponent<EnterEditController>();
            //uiController.SetCanvas(gameObject);
//#else



            EnterEditController uiController = EditUI.GetComponent<EnterEditController>();
            uiController.SetCanvas(gameObject);

            Canvas canvas = EditUI.GetComponent<Canvas>();
            canvas.enabled = true;

            if (canvas.worldCamera == null)
            {
                 canvas.worldCamera = localCamera;
            }


//#endif
            }
        }

    public void OnTriggerExit(Collider other)
    {
//#if UNITY_ANDROID
        Canvas tool_canvas = ToolUI.GetComponent<Canvas>();
        tool_canvas.enabled = false;
//#else
        Canvas canvas = EditUI.GetComponent<Canvas>();
        canvas.enabled = false;
        // Destroy UI and remove from dictionary
        //if (activeUIs.ContainsKey(other))
        //{
        //    Destroy(activeUIs[other]);
        //    activeUIs.Remove(other);

        //    Debug.Log($"UI removed from {other.name}");
        //}
//#endif
    }
}

