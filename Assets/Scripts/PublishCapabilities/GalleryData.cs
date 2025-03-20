using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class GalleryData
{
    public List<string> imageURLs;
}

public class UnityCloudGallery : MonoBehaviour
{
    // Call this coroutine to retrieve the gallery.
    public IEnumerator RetrieveGallery()
    {
        // Replace with your actual endpoint that returns gallery data.
        string galleryURL = "https://your_unity_cloud_endpoint/gallery";

        using (UnityWebRequest www = UnityWebRequest.Get(galleryURL))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error retrieving gallery: " + www.error);
            }
            else
            {
                string jsonResult = www.downloadHandler.text;
                Debug.Log("Gallery JSON: " + jsonResult);

                // Parse the JSON into a GalleryData object.
                GalleryData data = JsonUtility.FromJson<GalleryData>(jsonResult);

                // Example: Log each URL.
                foreach (string url in data.imageURLs)
                {
                    Debug.Log("Image URL: " + url);
                }
            }
        }
    }
}
