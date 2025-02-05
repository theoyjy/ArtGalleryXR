using UnityEngine;

public class EditCanvasUIController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject Canvas;
    async void Start()
    {
        UnityEngine.UI.Button targetButton = GetComponentInChildren<UnityEngine.UI.Button>();

        if (targetButton == null)
        {
            Debug.LogError("Button not found in " + gameObject.name);
            return;
        }
        if (CanvasEditManager.Instance == null)
        {
            Debug.LogError("CanvasEditManager not found");
            return;
        }
        targetButton.onClick.AddListener(() => CanvasEditManager.Instance.EnterEditMode(gameObject));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetCanvas(GameObject canvas)
    { 
        Canvas = canvas;
        Debug.Log(canvas.name + " is with " + gameObject.name);
    }
}
