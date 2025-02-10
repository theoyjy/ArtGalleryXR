using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Unity.Services.Vivox;
using System.Text.RegularExpressions;

public class LoginScreenUI : MonoBehaviour
{
    public Button LoginButton;
    public InputField DisplayNameInput;
    public GameObject LoginScreen;

    const int k_DefaultMaxStringLength = 15;

    int m_PermissionAskedCount;
    EventSystem m_EventSystem;

    void Start()
    {
        StartCoroutine(Setup());
    }

    IEnumerator Setup()
    {
        m_EventSystem = FindObjectOfType<EventSystem>();
        yield return new WaitUntil(() => VivoxService.Instance != null);

        VivoxService.Instance.LoggedIn += OnUserLoggedIn;
        VivoxService.Instance.LoggedOut += OnUserLoggedOut;

#if !(UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL)
        DisplayNameInput.interactable = false;
#else
        DisplayNameInput.onEndEdit.AddListener((string text) => { LoginToVivoxService(); });
#endif
        LoginButton.onClick.AddListener(() => { LoginToVivoxService(); });

        var systInfoDeviceName = string.IsNullOrWhiteSpace(SystemInfo.deviceName) == false ? SystemInfo.deviceName : Environment.MachineName;
        var resizedDisplayName = systInfoDeviceName.Substring(0, Math.Min(k_DefaultMaxStringLength, systInfoDeviceName.Length));
        DisplayNameInput.text = string.IsNullOrEmpty(resizedDisplayName) ? "Temp" : resizedDisplayName;
        Debug.Log(DisplayNameInput.text);
        LoginToVivoxService();
    }

    void OnDestroy()
    {
        VivoxService.Instance.LoggedIn -= OnUserLoggedIn;
        VivoxService.Instance.LoggedOut -= OnUserLoggedOut;

        LoginButton.onClick.RemoveAllListeners();
#if UNITY_STANDALONE || UNITY_IOS || UNITY_ANDROID || UNITY_WEBGL
        DisplayNameInput.onEndEdit.RemoveAllListeners();
#endif
    }

    void ShowLoginUI()
    {
        LoginScreen.SetActive(true);
        LoginButton.interactable = true;
        m_EventSystem.SetSelectedGameObject(LoginButton.gameObject, null);
    }

    void HideLoginUI()
    {
        LoginScreen.SetActive(false);
    }

#if (UNITY_ANDROID && !UNITY_EDITOR) || __ANDROID__
    bool IsAndroid12AndUp()
    {
        const int android12VersionCode = 31;
        AndroidJavaClass buildVersionClass = new AndroidJavaClass("android.os.Build$VERSION");
        int buildSdkVersion = buildVersionClass.GetStatic<int>("SDK_INT");
        return buildSdkVersion >= android12VersionCode;
    }

    string GetBluetoothConnectPermissionCode()
    {
        if (IsAndroid12AndUp())
        {
            AndroidJavaClass manifestPermissionClass = new AndroidJavaClass("android.Manifest$permission");
            string permissionCode = manifestPermissionClass.GetStatic<string>("BLUETOOTH_CONNECT");
            return permissionCode;
        }
        return "";
    }
#endif

    bool IsMicPermissionGranted()
    {
        bool isGranted = Permission.HasUserAuthorizedPermission(Permission.Microphone);
#if (UNITY_ANDROID && !UNITY_EDITOR) || __ANDROID__
        if (IsAndroid12AndUp())
        {
            isGranted &= Permission.HasUserAuthorizedPermission(GetBluetoothConnectPermissionCode());
        }
#endif
        return isGranted;
    }

    void AskForPermissions()
    {
        string permissionCode = Permission.Microphone;
#if (UNITY_ANDROID && !UNITY_EDITOR) || __ANDROID__
        if (m_PermissionAskedCount == 1 && IsAndroid12AndUp())
        {
            permissionCode = GetBluetoothConnectPermissionCode();
        }
#endif
        m_PermissionAskedCount++;
        Permission.RequestUserPermission(permissionCode);
    }

    bool IsPermissionsDenied()
    {
#if (UNITY_ANDROID && !UNITY_EDITOR) || __ANDROID__
        if (IsAndroid12AndUp())
        {
            return m_PermissionAskedCount == 2;
        }
#endif
        return m_PermissionAskedCount == 1;
    }

    void LoginToVivoxService()
    {
        if (IsMicPermissionGranted())
        {
            LoginToVivox();
        }
        else
        {
            if (IsPermissionsDenied())
            {
                m_PermissionAskedCount = 0;
                LoginToVivox();
            }
            else
            {
                AskForPermissions();
            }
        }
    }

    async void LoginToVivox()
    {
        LoginButton.interactable = false;

        var correctedDisplayName = Regex.Replace(DisplayNameInput.text, "[^a-zA-Z0-9_-]", "");
        DisplayNameInput.text = correctedDisplayName.Substring(0, Math.Min(correctedDisplayName.Length, 30));
        if (string.IsNullOrEmpty(DisplayNameInput.text))
        {
            Debug.LogError("Please enter a display name.");
            return;
        }
        await VivoxVoiceManager.Instance.InitializeAsync(DisplayNameInput.text);
        var loginOptions = new LoginOptions()
        {
            DisplayName = DisplayNameInput.text,
            ParticipantUpdateFrequency = ParticipantPropertyUpdateFrequency.FivePerSecond
        };
        await VivoxService.Instance.LoginAsync(loginOptions);
    }

    void OnUserLoggedIn()
    {
        HideLoginUI();
    }

    void OnUserLoggedOut()
    {
        ShowLoginUI();
    }
}
