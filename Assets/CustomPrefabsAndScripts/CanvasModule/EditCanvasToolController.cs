using UnityEngine;

public class EditCanvasToolController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UnityEngine.UI.Button[] buttons = GetComponentsInChildren<UnityEngine.UI.Button>();
        if(buttons.Length == 0)
        {
            Debug.LogError("Button not found in " + gameObject.name);
            return;
        }

        foreach (UnityEngine.UI.Button button in buttons)
        {
            if(button.name == "ExitBtn")
                button.onClick.AddListener(() => ExitEditMode());
            else if(button.name == "DeleteBtn")
                button.onClick.AddListener(() => CanvasEditManager.Instance.DeleteCanvas());
            //else if(button.name == "SaveBtn")
            //    button.onClick.AddListener(() => CanvasEditManager.Instance.SaveCanvas());
            else
                Debug.LogError("Button not found in " + gameObject.name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void ExitEditMode()
    {
        CanvasEditManager.Instance.ExitEditMode();
        Canvas canvas= gameObject.GetComponent<Canvas>();
        canvas.enabled = false;
    }
}
