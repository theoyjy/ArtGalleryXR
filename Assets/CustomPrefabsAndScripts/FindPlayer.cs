using UnityEngine;
using UnityEngine.InputSystem.UI;
using System.Collections;

public class FindPlayer : MonoBehaviour
{

    private InputSystemUIInputModule inputModule;
    private bool isAssigned = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inputModule = GetComponent<InputSystemUIInputModule>();
        StartCoroutine(FindXRTrackingOrigin());
    }

    IEnumerator FindXRTrackingOrigin()
    {
        while (!isAssigned)
        {
            // Look for a GameObject tagged as "Player"
            GameObject player = GameObject.FindGameObjectWithTag("Player");

            if (player != null)
            {
                inputModule.xrTrackingOrigin = player.transform;
                isAssigned = true;
                Debug.Log("XR Tracking Origin assigned to: " + player.name);
            }
            else
            {
                Debug.LogWarning("Searching for Player...");
            }

            yield return new WaitForSeconds(1f); // Check every second to avoid performance issues
        }
    }
}
