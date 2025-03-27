using UnityEngine;
using UnityEngine.XR;
using UnityEngine.SceneManagement;

public class PlatformChecker : MonoBehaviour
{
    private static bool _alreadyChecked = false;  // 防止重复检测

    private void Awake()
    {
        if (_alreadyChecked)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);  // 保证不会因为场景切换而被销毁
        _alreadyChecked = true;

        CheckPlatformAndLoadScene();
    }

    private void CheckPlatformAndLoadScene()
    {
        if (IsVRDevicePresent())
        {
            Debug.Log("VR Headset detected. Loading VR Scene...");
            SceneManager.LoadScene("LoginUI");  // 修改为你的 VR 场景名称
        }
        else
        {
            Debug.Log("Running on Desktop. Loading Desktop Scene...");
            SceneManager.LoadScene("Login");  // 修改为你的桌面场景名称
        }
    }

    private bool IsVRDevicePresent()
    {
        // 检查是否有启用的 XR Loader
        if (XRSettings.isDeviceActive)  // 如果正在使用旧版 Unity XR API
        {
            return true;
        }

        return false;
    }
}
