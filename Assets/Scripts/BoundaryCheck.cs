using UnityEngine;

public class BoundaryCheck : MonoBehaviour
{
    private Vector3 lastValidPosition;

    void Start()
    {
        lastValidPosition = GameObject.FindWithTag("Player").transform.position;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            lastValidPosition = other.transform.position; // Store the last safe position
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            other.transform.position = lastValidPosition; // Reset player position
        }
    }
}
