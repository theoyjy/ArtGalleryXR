using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class UnityCloudUploader : MonoBehaviour
{
    // Call this method with your canvas image (Texture2D) to upload.
    public IEnumerator UploadImage(Texture2D imageTexture)
    {
        // Convert the texture to PNG.
        byte[] imageData = imageTexture.EncodeToPNG();

        // Prepare the form data.
        WWWForm form = new WWWForm();
        form.AddBinaryData("file", imageData, "canvasImage.png", "image/png");

        // Replace this URL with your actual Unity Cloud upload endpoint.
        string uploadURL = "https://your_unity_cloud_endpoint/upload";

        using (UnityWebRequest www = UnityWebRequest.Post(uploadURL, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Upload failed: " + www.error);
            }
            else
            {
                // Assume the server returns the URL of the uploaded image in plain text.
                string imageURL = www.downloadHandler.text;
                Debug.Log("Image uploaded successfully. URL: " + imageURL);

                // You can now broadcast this URL to other players or add it to your gallery.
            }
        }
    }
}
