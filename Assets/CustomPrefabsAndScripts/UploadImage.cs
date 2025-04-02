using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;

public class UploadImage : MonoBehaviour
{
    public MeshRenderer targetRenderer;  // Assign in Inspector
    private string projectId; // Replace with actual Unity project ID
    private string environmentId; // Replace with CCD environment ID
    private string bucketId; // Replace with CCD bucket ID
    private string keyId; // Replace with Unity API key ID
    private string apiKey; // Replace with Unity API secret key

    private string baseUrl = "https://services.api.unity.com/ccd/management/v1/projects/";

    public void UploadTexture()
    {
        if (targetRenderer == null || targetRenderer.material == null || targetRenderer.material.mainTexture == null)
        {
            Debug.LogError("MeshRenderer or its texture is missing!");
            return;
        }

        Texture2D texture = targetRenderer.material.mainTexture as Texture2D;
        if (texture == null)
        {
            Debug.LogError("Texture is not a Texture2D!");
            return;
        }

        StartCoroutine(UploadToCCD(texture));
    }

    private IEnumerator UploadToCCD(Texture2D texture)
    {
        apiKey = Environment.GetEnvironmentVariable("UNITY_CCD_API_KEY");
        projectId = "d6be0271-d571-42d8-a09b-305d7f148d1f";
        environmentId = "a692e5af-8842-4289-bb91-ad105fd3e95d";
        bucketId = "29e6096a-5d6c-4985-a63c-ed8a16000af9";
        keyId = "98bebcbf-7703-4d7a-91a0-c763086a168d";
        
        byte[] pngData = texture.EncodeToPNG();
        if (pngData == null)
        {
            Debug.LogError("Failed to encode texture to PNG.");
            yield break;
        }

        string fileName = "mesh_texture_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".png";
        string contentHash = CalculateMD5(pngData);
        string fullUrl = $"{baseUrl}{projectId}/environments/{environmentId}/buckets/{bucketId}/entries/";


        // Create JSON payload
        string jsonPayload = $@"
        {{
            ""path"": ""{fileName}"",
            ""content_hash"": ""{contentHash}"",
            ""content_size"": {pngData.Length},
            ""content_type"": ""image/png"",
            ""signed_url"": true
        }}";

        using (UnityWebRequest request = new UnityWebRequest(fullUrl, "POST"))
        {
            byte[] jsonBytes = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(jsonBytes);
            request.downloadHandler = new DownloadHandlerBuffer();

            string authHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{keyId}:{apiKey}"));
            request.SetRequestHeader("Authorization", "Basic " + authHeader);
            //request.SetRequestHeader("Authorization", "Bearer " + apiKey);
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Upload Request Success: " + request.downloadHandler.text);
                string signedUrl = ExtractSignedUrl(request.downloadHandler.text);
                if (!string.IsNullOrEmpty(signedUrl))
                {
                    Debug.Log("Uploading file to signed URL...");
                    StartCoroutine(UploadToSignedUrl(signedUrl, pngData));
                }

                string download_url = ExtractContentLink(request.downloadHandler.text);
                Debug.Log(download_url);
                
            }
            else
            {
                Debug.LogError("Upload Request Failed: " + request.error);
            }
        }
    }

    private IEnumerator UploadToSignedUrl(string signedUrl, byte[] pngData)
    {
        using (UnityWebRequest uploadRequest = UnityWebRequest.Put(signedUrl, pngData))
        {
            uploadRequest.SetRequestHeader("Content-Type", "image/png");
            yield return uploadRequest.SendWebRequest();

            if (uploadRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("File successfully uploaded to CCD!");
            }
            else
            {
                Debug.LogError("Upload to Signed URL Failed: " + uploadRequest.error);
            }
        }
    }

    private string CalculateMD5(byte[] data)
    {
        using (MD5 md5 = MD5.Create())
        {
            byte[] hashBytes = md5.ComputeHash(data);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }
    }

    private string ExtractSignedUrl(string jsonResponse)
    {
        try
        {
            // Extract "signed_url" from the JSON response (manual parsing)
            int index = jsonResponse.IndexOf("\"signed_url\":\"");
            if (index == -1) return null;

            int start = index + 14;
            int end = jsonResponse.IndexOf("\"", start);
            if (end == -1) return null;

            string signedUrl = jsonResponse.Substring(start, end - start);
            return signedUrl.Replace("\\/", "/"); // Fix JSON escaped slashes
        }
        catch (Exception e)
        {
            Debug.LogError("Error parsing signed URL: " + e.Message);
            return null;
        }
    }

    private string ExtractContentLink(string jsonResponse)
    {
        try
        {
            var json = JObject.Parse(jsonResponse);
            return json["content_link"]?.ToString(); // ✅ Extract direct download URL
        }
        catch (Exception e)
        {
            Debug.LogError("Error parsing content link: " + e.Message);
            return null;
        }
    }
}
