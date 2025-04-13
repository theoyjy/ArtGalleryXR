using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class CanvasUpdater : MonoBehaviour
{
    public Material[] materials;
    public Texture2D currentCanvasTexture;

    private string previousTextureUrl = "."; 
    public string currentTextureUrl = "";

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        materials = GetComponent<Renderer>().materials;
    }

    // Update is called once per frame
    async void Update()
    {
        if (previousTextureUrl != currentTextureUrl)
        {
            StartCoroutine(fetchTexture(currentTextureUrl));
        }
        previousTextureUrl = currentTextureUrl;
    }

    IEnumerator fetchTexture(string url)
    {
        using (UnityWebRequest req = UnityWebRequestTexture.GetTexture(url))
        {
            yield return req.SendWebRequest();
            if (req.result == UnityWebRequest.Result.Success)
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(req);
                currentCanvasTexture = texture;
                materials[0].mainTexture = currentCanvasTexture;
            }
            else
            {
                Debug.Log("texture download failed");
            }
            
        }
    }
}
