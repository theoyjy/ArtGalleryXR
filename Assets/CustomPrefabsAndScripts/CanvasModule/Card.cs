using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour
{
    private SpriteRenderer rend;

    [SerializeField]
    private Sprite EmptySprite;

    private bool coroutineAllowed;

    // Start is called before the first frame update
    void Start()
    {
        rend = GetComponent<SpriteRenderer>();
        rend.sprite = EmptySprite;
        coroutineAllowed = true;
    }

    public void DeleteDrawing()
    {
        Debug.Log("DeleteDrawing");
        StartCoroutine(RotateCard());

        Whiteboard wbm = transform.parent.GetComponentInChildren<Whiteboard>();
        wbm.ClearWhiteboard();

        try
        {
            transform.parent.GetComponentInChildren<TextureSyncManager>().SendClearWhiteboardOprServerRpc();
        }
        catch(System.Exception e)
        {
            Debug.LogWarning("TextureSyncManager not found on Card or its children.");
        }
    }

    private IEnumerator RotateCard()
    {
        Debug.Log("Trigger Canvas Flip");
        coroutineAllowed = false;

        for (float i = 180f; i >= 0f; i -= 10f)
        {
            transform.localRotation = Quaternion.Euler(-7.7f, i, 0f);
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
        return transform.forward;
    }

}
