using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;

public class PlayFabSharedGroupDemo : MonoBehaviour
{
    void Start()
    {
        // 登录 PlayFab（匿名登录）
        Login();
    }

    // 使用自定义 ID 进行登录，并自动创建账号
    void Login()
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        };

    }

    void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("登录成功，PlayFab ID: " + result.PlayFabId);

    }

}