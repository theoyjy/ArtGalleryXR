using UnityEngine;

public class ExitTrigger : MonoBehaviour
{
    public GameObject exitButton; 

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Trigger Enter");
        if (other.CompareTag("Player"))
        {
            exitButton.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Trigger Exit");
        if (other.CompareTag("Player"))
        {
            exitButton.SetActive(false);
        }
    }
}
