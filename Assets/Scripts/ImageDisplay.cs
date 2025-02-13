using UnityEngine;

public class ImageDisplay : MonoBehaviour
{
    public GeminiAI geminiAI;
    public Renderer targetRenderer;

    public async void OnGenerateImageButtonClick()
    {
        string prompt = "A beautiful sunset over the mountains";
        Texture2D generatedImage = await geminiAI.GenerateImage(prompt);
        if (generatedImage != null)
        {
            targetRenderer.material.mainTexture = generatedImage;
        }
    }
}
