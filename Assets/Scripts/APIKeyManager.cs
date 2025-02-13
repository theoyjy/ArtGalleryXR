using UnityEngine;
using System.Net.Http;

public class APIKeyManager : MonoBehaviour
{
    void Start()
    {
        PlayerPrefs.SetString("OpenAI_API_Key", "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");
        PlayerPrefs.Save();
        Debug.Log("API Key saved£¡");
        Debug.Log("API Key: " + PlayerPrefs.GetString("OpenAI_API_Key", "can not find API_KEY"));
    }
}
