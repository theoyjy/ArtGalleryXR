using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class TestAddCanvaSimple : MonoBehaviour
{
    [Header("PlayFab Login Info")]
    public string TitleId = "D41B8";   // 在 Inspector 填你的 TitleId
    public string CustomId = "TestUser123"; // 一个用来登录的自定义ID

    [Header("AddCanvaSimple Info")]
    public string GalleryId = "test";
    public string CanvaUrl = "https://example.com/myCanva.png";

    private void Start()
    {
        // 确保设置了 PlayFab TitleId
        if (!string.IsNullOrEmpty(TitleId))
        {
            PlayFabSettings.TitleId = TitleId;
        }

        // 尝试使用 CustomID 登录（没有账号会自动创建）
        var request = new LoginWithCustomIDRequest
        {
            CustomId = CustomId,
            CreateAccount = true
        };

        Debug.Log("开始登录 PlayFab...");
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log($"登录成功，PlayFabId: {result.PlayFabId}");
        Debug.Log("现在调用 AddCanvaSimple...");

        // 登录成功后，测试 AddCanvaSimple
        // 假设 ShareDataManager 是你写好的管理类，里边有 AddCanvaSimple
        SharedDataManager.AddCanvaSimple(GalleryId, CanvaUrl);
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogError("登录失败: " + error.GenerateErrorReport());
    }
}
