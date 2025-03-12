using UnityEngine;
using System.Collections.Generic;
using PlayFab;
using PlayFab.ClientModels;

[System.Serializable]
public class MyData
{
    public int level;
    public string characterName;
}

public class PlayFabDataStorageDemo : MonoBehaviour
{
    // 设定初始数据
    public MyData myData = new MyData { level = 1, characterName = "Hero" };

    void Start()
    {
        // 登录 PlayFab（这里使用匿名登录，实际项目中可根据需求更换登录方式）
        Login();
    }

    // 使用自定义设备 ID 进行登录，并自动创建账号（首次登录时创建账号）
    void Login()
    {
        var request = new LoginWithCustomIDRequest
        {
            CustomId = SystemInfo.deviceUniqueIdentifier,
            CreateAccount = true
        };
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnError);
    }

    // 登录成功回调
    void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("登录成功, PlayFab ID: " + result.PlayFabId);
        // 登录成功后保存数据
        SaveMyData();
    }

    // 将自定义数据序列化成 JSON 字符串后保存到 PlayFab 的 User Data
    void SaveMyData()
    {
        string json = JsonUtility.ToJson(myData);
        Debug.Log("保存数据: " + json);

        var request = new UpdateUserDataRequest
        {
            Data = new Dictionary<string, string> {
                { "MyCustomData", json }
            }
        };

        PlayFabClientAPI.UpdateUserData(request, OnDataUpdate, OnError);
    }

    // 数据更新成功后的回调，更新成功后读取数据进行验证
    void OnDataUpdate(UpdateUserDataResult result)
    {
        Debug.Log("数据更新成功");
        // 更新完成后加载数据以确认存储正确
        LoadMyData();
    }

    // 读取数据
    void LoadMyData()
    {
        var request = new GetUserDataRequest();
        PlayFabClientAPI.GetUserData(request, OnDataReceived, OnError);
    }

    // 读取数据成功后的回调，将 JSON 数据反序列化回自定义对象
    void OnDataReceived(GetUserDataResult result)
    {
        if (result.Data != null && result.Data.ContainsKey("MyCustomData"))
        {
            string json = result.Data["MyCustomData"].Value;
            Debug.Log("读取到数据: " + json);

            // 反序列化回 MyData 对象
            MyData loadedData = JsonUtility.FromJson<MyData>(json);
            Debug.Log("玩家等级: " + loadedData.level + " 角色名称: " + loadedData.characterName);
        }
        else
        {
            Debug.Log("没有找到数据");
        }
    }

    // 错误回调
    void OnError(PlayFabError error)
    {
        Debug.LogError("错误: " + error.GenerateErrorReport());
    }
}
