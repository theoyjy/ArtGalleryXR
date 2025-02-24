using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Vivox;
using System;
using System.Threading.Tasks;
#if AUTH_PACKAGE_PRESENT
using Unity.Services.Authentication;
#endif

public class VivoxVoiceManager : MonoBehaviour
{
    public const string LobbyChannelName = "lobbyChannel";

    // Check to see if we're about to be destroyed.
    static object m_Lock = new object();
    static VivoxVoiceManager m_Instance;

    //These variables should be set to the projects Vivox credentials if the authentication package is not being used
    //Credentials are available on the Vivox Developer Portal (developer.vivox.com) or the Unity Dashboard (dashboard.unity3d.com), depending on where the organization and project were made
    [SerializeField]
    string _key;
    [SerializeField]
    string _issuer;
    [SerializeField]
    string _domain;
    [SerializeField]
    string _server;

    /// <summary>
    /// Access singleton instance through this propriety.
    /// </summary>
    public static VivoxVoiceManager Instance
    {
        get
        {
            lock (m_Lock)
            {
                if (m_Instance == null)
                {
                    // Search for existing instance.
                    Debug.Log("Searching for existing VivoxVoiceManager instance...");
                    m_Instance = (VivoxVoiceManager)FindObjectOfType(typeof(VivoxVoiceManager));

                    // Create new instance if one doesn't already exist.
                    if (m_Instance == null)
                    {
                        // Need to create a new GameObject to attach the singleton to.
                        Debug.Log("No existing instance found. Creating a new one...");
                        var singletonObject = new GameObject();
                        m_Instance = singletonObject.AddComponent<VivoxVoiceManager>();
                        singletonObject.name = typeof(VivoxVoiceManager).ToString() + " (Singleton)";
                    }
                    else
                    {
                        Debug.Log("Build m_Instance successfully");
                    }
                }
                // Make instance persistent even if its already in the scene
                DontDestroyOnLoad(m_Instance.gameObject);
                return m_Instance;
            }
        }
    }

    async void Awake()
    {
        // Singleton initialization
        if (m_Instance == null)
        {
            var _ = Instance;
            if (_ == null)
            {
                Debug.LogError("m_Instance is null.");
            }
        }
        if (m_Instance != this && m_Instance != null)
        {
            Debug.LogWarning("Multiple VivoxVoiceManager instances detected. Only one will be kept.");
            Destroy(this);
            return;
        }

        // Set up initialization options with credentials if provided.
        var options = new InitializationOptions();
        if (CheckManualCredentials())
        {
            options.SetVivoxCredentials(_server, _domain, _issuer, _key);
            Debug.Log($"Server: {_server}, Domain: {_domain}, Issuer: {_issuer}, Key: {_key}");
        }

        // Initialize Unity Services
        try
        {
            await UnityServices.InitializeAsync(options);
            Debug.Log("UnityServices initialized successfully.");
        }
        catch (Exception ex)
        {
            Debug.LogError($"UnityServices initialization failed: {ex.Message}");
            return;
        }

        // Optionally log UnityServices state
        Debug.Log($"UnityServices state: {UnityServices.State}");

        // Wait for VivoxService.Instance to become available (with a timeout)
        const int maxAttempts = 20;
        int attempt = 0;
        while (VivoxService.Instance == null && attempt < maxAttempts)
        {
            Debug.Log("Waiting for VivoxService.Instance to become available...");
            await Task.Delay(500); // wait half a second
            attempt++;
        }

        if (VivoxService.Instance == null)
        {
            Debug.LogError("VivoxService.Instance is null. Ensure UnityServices initialization has completed and your Vivox configuration is correct.");
            return;
        }
        else
        {
            Debug.Log("VivoxService.Instance is available.");
        }

        // Proceed with any additional Vivox initialization logic here.
    }


    public async Task InitializeAsync(string playerName)
    {

#if AUTH_PACKAGE_PRESENT
        if (!CheckManualCredentials())
        {
            AuthenticationService.Instance.SwitchProfile(playerName);
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
#endif
    }

    bool CheckManualCredentials()
    {
        if (_server == null || _domain == null || _issuer == null || _key == null)
        {
            Debug.LogError($"Credentials are missing. Server: {_server}, Domain: {_domain}, Issuer: {_issuer}, Key: {_key}");
        }
        return !(string.IsNullOrEmpty(_issuer) && string.IsNullOrEmpty(_domain) && string.IsNullOrEmpty(_server));
    }
}
