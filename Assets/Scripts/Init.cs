using UnityEngine;
using UnityEngine.XR;
using UnityEngine.SceneManagement;

public class PlatformChecker : MonoBehaviour
{
    private static bool _alreadyChecked = false;  // ��ֹ�ظ����

    private void Awake()
    {
        if (_alreadyChecked)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);  // ��֤������Ϊ�����л���������
        _alreadyChecked = true;

        CheckPlatformAndLoadScene();
    }

    private void CheckPlatformAndLoadScene()
    {
        if (IsVRDevicePresent())
        {
            Debug.Log("VR Headset detected. Loading VR Scene...");
            SceneManager.LoadScene("LoginUI");  // �޸�Ϊ��� VR ��������
        }
        else
        {
            Debug.Log("Running on Desktop. Loading Desktop Scene...");
            SceneManager.LoadScene("Login");  // �޸�Ϊ������泡������
        }
    }

    private bool IsVRDevicePresent()
    {
        // ����Ƿ������õ� XR Loader
        if (XRSettings.isDeviceActive)  // �������ʹ�þɰ� Unity XR API
        {
            return true;
        }

        return false;
    }
}
