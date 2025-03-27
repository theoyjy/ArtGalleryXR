using TMPro;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Text;
using System;
using UnityEngine.UI;

public class StabilityAIImage : MonoBehaviour
{
    public string apiKey;
    public Renderer targetRenderer;
    public string prompt;
    public TMP_InputField generateImageInputField;

    public TextureSyncManager textureSyncManager;
    public Whiteboard whiteboard;


    void Start()
    {
        //Run CMD. setx STABILITY_API_KEY "your-api-key"
        //StartCoroutine(GenerateAIImage());
        
        if (generateImageInputField != null)
        {
            Debug.Log("Setting Listener");
            generateImageInputField.onEndEdit.AddListener(OnInputFieldSubmitted);

           
            //generateImageInputField.onSubmit.AddListener(OnInputFieldSubmitted);
        }
    }

    public void OnInputFieldSubmitted(string text)
    {

        if (!string.IsNullOrEmpty(text))  // Check if there's input
        {
            Debug.Log("Input Submitted: " + text);
            
            
            StartCoroutine(GenerateAIImage(text));  
            
            
        }
        else
        {
            Debug.LogError("No Input text");
        }
    }


    public IEnumerator GenerateAIImage(string prompt)
    {
        this.prompt = prompt;
        apiKey = "sk-uid5gUNQFyUvpwNUIzIfCC36dmgorwiIknlW3yR895Jydkih";
        Debug.Log("GenerateAIImage with prompt: "+prompt);
        Debug.Log(apiKey);
        string url = "https://api.stability.ai/v2beta/stable-image/generate/core";

        string boundary = "----UnityBoundary" + System.Guid.NewGuid().ToString();

        // Construct form-data manually
        StringBuilder formData = new StringBuilder();
        formData.AppendLine($"--{boundary}");
        formData.AppendLine("Content-Disposition: form-data; name=\"prompt\"");
        formData.AppendLine();
        formData.AppendLine(prompt);

        formData.AppendLine($"--{boundary}");
        formData.AppendLine("Content-Disposition: form-data; name=\"output_format\"");
        formData.AppendLine();
        formData.AppendLine("jpeg");
        formData.AppendLine($"--{boundary}--");

        byte[] bodyRaw = Encoding.UTF8.GetBytes(formData.ToString());
        //string json = JsonConvert.SerializeObject(requestBody);
        //byte[] jsonData = Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        //request.uploadHandler = new UploadHandlerRaw(jsonData);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("authorization", "Bearer " + apiKey);
        request.SetRequestHeader("content-type", $"multipart/form-data; boundary={boundary}");
        request.SetRequestHeader("accept", "image/*");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("AI Image Request Success: " + request.downloadHandler.text);
            //Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            //targetRenderer.material.mainTexture = texture;
            //var response = JsonConvert.DeserializeObject<StabilityResponse>(request.downloadHandler.text);
            //string imageUrl = response.image; // API 返回的图像 URL
            //StartCoroutine(DownloadImage(imageUrl));

            string contentType = request.GetResponseHeader("Content-Type");

            if (contentType != null && contentType.StartsWith("image"))
            {
                Debug.Log("AI Image Request Success: Received Image");

                // Now re-download the image properly as a texture
                StartCoroutine(DownloadImage(request.downloadHandler.data));
            }
            else
            {
                Debug.LogError("AI Image Request Failed: Response is not an image. Response Data: " + request.downloadHandler.text);
            }
        }
        else
        {
            Debug.Log(request.downloadHandler.text);
            Debug.LogError("AI Image Request Failed: " + request.error);
        }
    }

    IEnumerator DownloadImage(byte[] imageData)
    {
        //UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl);
        //yield return request.SendWebRequest();

        //if (request.result == UnityWebRequest.Result.Success)
        //{
        //    Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
        //    targetRenderer.material.mainTexture = texture;
        //}
        //else
        //{
        //    Debug.LogError("Failed to download AI image: " + request.error);
        //}

        Texture2D texture = new Texture2D(2, 2);

        if (texture.LoadImage(imageData))
        {
            Debug.Log("Setting Texture");
            texture = whiteboard.HandleFlip(texture);
            whiteboard.texture = texture;
            targetRenderer.material.mainTexture = texture;
            textureSyncManager.SendTextureToServer();
        }
        else
        {
            Debug.LogError("Failed to convert response data to Texture2D.");
        }
        yield return null;
    }
}


// 用于 JSON 解析的类
[System.Serializable]
public class StabilityResponse
{
    public string image;
}
