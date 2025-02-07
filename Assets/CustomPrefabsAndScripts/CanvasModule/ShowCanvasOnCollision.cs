using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
// If you're using Mirror or Netcode for GameObjects, uncomment accordingly:
// using Mirror;

public class ShowCanvasOnCollision : MonoBehaviour
{
    [Header("UI Prefab (Must be a World-Space Canvas)")]
    [SerializeField] private GameObject UI;

    [Header("Tag to Detect (e.g., 'Interactable')")]
    //[SerializeField] private string interactableTag = "Interactable";
    [SerializeField] private string interactableTag = "Player";

    [Header("Spawn Offset from Collision (Optional)")]
    [SerializeField] public float spawnOffset = 0.1f;


    private Dictionary<Collider, GameObject> activeUIs = new Dictionary<Collider, GameObject>();


    // If using a networking framework that distinguishes local players:
    // private bool isLocalPlayer = true; // Or if using Mirror: [SyncVar] public bool isLocalPlayer;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(" Trigger entered with: " + other.tag);
        if(other.CompareTag(interactableTag))
        //if (interactableTag.CompareTo(other.name) == 1)
        {
            Card card = GetComponentInChildren<Card>();
            if (card == null)
            {
                Debug.LogError("Card component is missing from: " + gameObject.name);
                return;
            }

            Vector3 spawnPosition = card.GetWorldLoc() + spawnOffset * card.GetNormal();
            Debug.Log("Spawn position: " + spawnPosition);

            // Spawn the UI *locally* (no network spawn)
            GameObject uiInstance = Instantiate(UI, spawnPosition, Quaternion.identity);

            EnterEditController uiController = uiInstance.GetComponent<EnterEditController>();
            Button myButton = uiInstance.transform.Find("Button").GetComponent<Button>();
            if(myButton == null)
            {
                Debug.LogError("Button component is missing from: " + uiInstance.name);
                return;
            }
            if(uiController == null)
            {
                Debug.LogError("UIController component is missing from: " + uiInstance.name);
            }

            // move the button to the spawn position
            myButton.transform.position = spawnPosition;

            // Store the canvas in the UI controller to later pass to the CanvasEditManager
            uiController.SetCanvas(gameObject);

            // add UI to dictionary
            if (activeUIs.ContainsKey(other))
            {
                Destroy(activeUIs[other]);
            }
            activeUIs[other] = uiInstance;

        }
    }

    public void OnTriggerExit(Collider other)
    {
        // Destroy UI and remove from dictionary
        if (activeUIs.ContainsKey(other))
        {
            Destroy(activeUIs[other]);
            activeUIs.Remove(other);

            Debug.Log($"UI removed from {other.name}");
        }
    }
}

