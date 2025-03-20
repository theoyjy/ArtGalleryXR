using UnityEngine;

public class PaintingTrigger : MonoBehaviour
{
    public GameObject saveButton; // Assign UI Button in Inspector

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            saveButton.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            saveButton.SetActive(false);
        }
    }
}
