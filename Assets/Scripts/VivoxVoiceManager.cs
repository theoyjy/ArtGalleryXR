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

    // Hardcoded Vivox credentials
    private const string HardcodedKey = "cPShxkTVcgFJETrtg93pYZDPKoIuRh0l";
    private const string HardcodedIssuer = "16767-artga-80378-udash";
    private const string HardcodedDomain = "mtu1xp.vivox.com";
    private const string HardcodedServer = "https://unity.vivox.com/appconfig/16767-artga-80378-udash";

    /// <summary>
    /// Access singleton instance through this property.
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
                    m_Instance = (VivoxVoiceManager)FindObjectOfType(typeof(VivoxVoiceManager));

                    // Create new instance if one doesn't already exist.
                    if (m_Instance == null)
                    {
                        // Need to create a new GameObject to attach the singleton to.
                        var singletonObject = new GameObject();
                        m_Instance = singletonObject.AddComponent<VivoxVoiceManager>();
                        singletonObject.name = typeof(VivoxVoiceManager).ToString() + " (Singleton)";
                    }
                }
                // Make instance persistent even if it's already in the scene
                DontDestroyOnLoad(m_Instance.gameObject);
                return m_Instance;
            }
        }
    }

    async void Awake()
    {
        if (m_Instance != this && m_Instance != null)
        {
            Debug.LogWarning(
                "Multiple VivoxVoiceManager detected in the scene. Only one VivoxVoiceManager can exist at a time. The duplicate VivoxVoiceManager will be destroyed.");
            Destroy(this);
        }

        var options = new InitializationOptions();

        // Set hardcoded Vivox credentials
        options.SetVivoxCredentials(HardcodedServer, HardcodedDomain, HardcodedIssuer, HardcodedKey);

        await UnityServices.InitializeAsync(options);
        await VivoxService.Instance.InitializeAsync();

        // Automatically log in and join a default channel
        await AutoJoinChannelAsync();
    }

    public async Task InitializeAsync(string playerName)
    {
#if AUTH_PACKAGE_PRESENT
        AuthenticationService.Instance.SwitchProfile(playerName);
        await AuthenticationService.Instance.SignInAnonymouslyAsync();
#endif
    }

    private async Task AutoJoinChannelAsync()
    {
        try
        {
            // Automatically log in (using anonymous login)
#if AUTH_PACKAGE_PRESENT
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
#endif
            var loginOptions = new LoginOptions
            {
                DisplayName = "Player_" + UnityEngine.Random.Range(1000, 9999), // Example: dynamic player display name
                ParticipantUpdateFrequency = ParticipantPropertyUpdateFrequency.FivePerSecond
            };
            await VivoxService.Instance.LoginAsync(loginOptions);

            Debug.Log($"Successfully joined channel: {LobbyChannelName}");
            await VivoxService.Instance.JoinGroupChannelAsync(LobbyChannelName, ChatCapability.TextAndAudio);

        }
        catch (Exception ex)
        {
            Debug.LogError($"Error joining Vivox channel: {ex.Message}");
        }
    }
}
