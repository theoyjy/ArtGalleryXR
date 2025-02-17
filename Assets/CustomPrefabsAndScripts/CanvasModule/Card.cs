using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    //private MeshRenderer rend;
    private MeshFilter meshFilter;
    //private UnityEngine.Plane plane;

    [SerializeField]
    private Mesh EmptyMesh;

    private bool coroutineAllowed;

    // Start is called before the first frame update
    void Start()
    {
        //rend = GetComponent<MeshRenderer>();
        //plane = GetComponent<UnityEngine.Plane>();
        //rend.mesh = EmptyMesh;
        
        // Get the MeshFilter component from the GameObject.
        meshFilter = GetComponent<MeshFilter>();
        if (meshFilter != null && EmptyMesh != null)
        {
            // Initialize the canvas with an empty mesh.
            meshFilter.mesh = EmptyMesh;
        }
        else
        {
            Debug.LogWarning("MeshFilter or EmptyMesh not assigned on " + gameObject.name);
        }

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

        Whiteboard whiteboard = GetComponentInChildren<Whiteboard>();
        if (whiteboard != null)
        {
            whiteboard.ClearWhiteboard();
        }
        else
        {
            Debug.LogWarning("Whiteboard component not found on Card or its children.");
        }

        ClearCanvas();
        StartCoroutine(RotateCard());
    }

    // Resets the MeshFilter's mesh to the empty mesh.
    private void ClearCanvas()
    {
        if (meshFilter != null && EmptyMesh != null)
        {
            meshFilter.mesh = EmptyMesh;
            Debug.Log("Canvas cleared");
        }
        else
        {
            Debug.LogWarning("Cannot clear canvas. MeshFilter or EmptyMesh not assigned.");
        }
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

    public Quaternion GetWorldQuatRot()
    {
        Quaternion quat = transform.parent.transform.rotation; // parent's world rotation
        quat *= Quaternion.Euler(0f, 180f, 0f);
        return quat;
    }

    public Vector3 GetNormal()
    {
        Quaternion quat = GetWorldQuatRot();
        return quat * Vector3.forward;
    }

}
