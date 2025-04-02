using UnityEngine;

public class AlignPlayer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Ensure the XR Origin starts at (0,0,0) so the body aligns correctly
        transform.position = Vector3.zero;
        transform.rotation = Quaternion.identity;
    }
}
