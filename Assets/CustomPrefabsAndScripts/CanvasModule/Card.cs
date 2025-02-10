using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    //private MeshRenderer rend;

    //private UnityEngine.Plane plane;

    //[SerializeField]
    //private Mesh EmptyMesh;

    private bool coroutineAllowed;

    // Start is called before the first frame update
    void Start()
    {
        //rend = GetComponent<MeshRenderer>();
        //plane = GetComponent<UnityEngine.Plane>();
        //rend.mesh = EmptyMesh;
        coroutineAllowed = true;
        lastClickTime = 0;
    }

    private float lastClickTime;
    private const float doubleClickTime = 0.3f; // Adjust the time interval for double-click detection

    private void OnMouseDown()
    {
        if(lastClickTime == 0)
        {
            lastClickTime = Time.time;
            return;
        }

        float timeSinceLastClick = Time.time - lastClickTime;
        if (timeSinceLastClick <= doubleClickTime && coroutineAllowed)
        {
            Debug.Log("Try Trigger Canvas Flip");
            StartCoroutine(RotateCard());
        }
        lastClickTime = Time.time;
    }

    public void DeleteDrawing()
    {
        Debug.Log("DeleteDrawing");
        StartCoroutine(RotateCard());
    }

    private IEnumerator RotateCard()
    {
        Debug.Log("Trigger Canvas Flip");
        coroutineAllowed = false;

        for (float i = 360f; i >= 180f; i -= 10f)
        {
            transform.localRotation = Quaternion.Euler(-90, i, 0f);
            yield return new WaitForSeconds(0.01f);
        }

        coroutineAllowed = true;
    }

    public Vector3 GetWorldLoc()
    {
        return transform.position;
    }

    public Vector3 GetNormal()
    {
        Vector3 normal = Quaternion.Euler(-90f, 180f, 0) * transform.forward;
        return normal;
    }

}
