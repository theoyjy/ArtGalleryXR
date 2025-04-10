using UnityEngine;
using UnityEngine.XR;
using UnityEngine.SceneManagement;

public class PlatformChecker : MonoBehaviour
{
    private static bool _alreadyChecked = false;

    private void Awake()
    {
        if (_alreadyChecked)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        _alreadyChecked = true;

        CheckPlatformAndLoadScene();
    }

    private void CheckPlatformAndLoadScene()
    {

#if UNITY_ANDROID
        //if (IsVRDevicePresent())
        //{
        Debug.Log("VR Headset detected. Loading VR Scene...");
        SceneManager.LoadScene("LoginUI");
        //}
#else
        //else
        //{
            Debug.Log("Running on Desktop. Loading Desktop Scene...");
            SceneManager.LoadScene("Login");
        //}
#endif
    }

    private bool IsVRDevicePresent()
    {
        if (XRSettings.isDeviceActive)
        {
            return true;
        }

        return false;
    }
}
