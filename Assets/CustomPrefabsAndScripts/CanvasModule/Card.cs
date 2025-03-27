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
