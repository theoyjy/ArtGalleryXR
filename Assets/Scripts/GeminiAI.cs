using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

public class GeminiAI : MonoBehaviour
{
    private static readonly string apiKey = "AIzaSyBaEiJsW3tnoQweraiwDZpoDnaFGJquWz0";
    private static readonly string apiUrl = "https://api.gemini.com/v1/generate-image";

    public async Task<Texture2D> GenerateImage(string prompt)
    {
        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var requestBody = new
            {
                prompt = prompt,
                // 您可以根据需要添加其他参数
            };

            string json = JsonConvert.SerializeObject(requestBody);
            StringContent content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await client.PostAsync(apiUrl, content);
            if (response.IsSuccessStatusCode)
            {
                string responseBody = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<GeminiResponse>(responseBody);
                byte[] imageBytes = Convert.FromBase64String(result.image_base64);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(imageBytes);
                return texture;
            }
            else
            {
                Debug.LogError($"Error: {response.ReasonPhrase}");
                return null;
            }
        }
    }
}

public class GeminiResponse
{
    public string image_base64 { get; set; }
}
