using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class CanvasUpdater : MonoBehaviour
{
    public bool is_Changed = false;
    public Material[] materials;
    public Texture2D current_canvas_texture;

    public string texture_url = "https://d6be0271-d571-42d8-a09b-305d7f148d1f.client-api.unity3dusercontent.com/client_api/v1/environments/production/buckets/29e6096a-5d6c-4985-a63c-ed8a16000af9/release_by_badge/latest/entry_by_path/content/?path=image.jpg";

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
            StartCoroutine(fetchTexture(texture_url));
            is_Changed = false;
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
                materials[0].mainTexture = current_canvas_texture;
            }
            else
            {

            }
            
        }
    }
}
