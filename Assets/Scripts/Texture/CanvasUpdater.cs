using System.Collections;
using Microsoft.Unity.VisualStudio.Editor;
using Unity.VisualScripting.ReorderableList.Element_Adder_Menu;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class CanvasUpdater : MonoBehaviour
{
    public bool is_Changed = false;
    public Material[] materials;
    public Texture2D current_canvas_texture;

    public RawImage texture_image;
    public string texture_url = "https://cdn.britannica.com/33/194733-050-4CF75F31/Girl-with-a-Pearl-Earring-canvas-Johannes-1665.jpg";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        materials = GetComponent<Renderer>().materials;
    }

    // Update is called once per frame
    async void Update()
    {
        if (is_Changed)
        {
            is_Changed = false;
            StartCoroutine(fetchTexture(texture_url));
            materials[1].mainTexture = current_canvas_texture;
        }
    }

    IEnumerator fetchTexture(string url)
    {
        using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(url))
        {
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(req);
                current_canvas_texture = texture;
            }
            else
            {

            }
        }
    }
}
