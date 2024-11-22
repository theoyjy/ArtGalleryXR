using UnityEngine;
using UnityEngine.Rendering;

public class CanvasUpdater : MonoBehaviour
{
    public bool is_Changed = false;
    public Material[] materials;
    public Texture2D current_canvas_texture;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        materials = GetComponent<Renderer>().materials;
    }

    // Update is called once per frame
    void Update()
    {
        if (is_Changed)
        {
            materials[1].mainTexture = current_canvas_texture;
            // is_Changed = false;
        }
    }
}
