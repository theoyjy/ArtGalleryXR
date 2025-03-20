using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using System.Collections.Generic;

public class PlayFabSharedGroupDemo : MonoBehaviour
{
    //// 指定一个 Shared Group ID（你也可以用随机字符串）
    //private string sharedGroupId = "TestSharedGroup";

    //// 测试数据（JSON 格式）
    //private string testDataJson = "{\"score\":100, \"message\":\"Hello Shared Group\"}";

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

        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnError);
    }

    void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("登录成功，PlayFab ID: " + result.PlayFabId);
        SharedDataManager.CreateSharedGroup(SharedDataManager.GallerySharedGroupId, () =>
        {
        }, error =>
        {
            Debug.LogError("创建 Gallery 共享组出错：" + error.GenerateErrorReport());
        });

        SharedDataManager.CreateSharedGroup(SharedDataManager.CanvaSharedGroupId, () =>
        {
        }, error =>
        {
            Debug.LogError("创建 Canva 共享组出错：" + error.GenerateErrorReport());
        });

        //向组中添加数据
        GalleryDetail newGallery = new GalleryDetail
        {
            GalleryID = "gallery001",
            GalleryName = "My First Gallery",
            Canva = new List<string> { "canva001", "canva002" },
            OwnID = "Player001",
            permission = "public"
        };

        SharedDataManager.SaveGallery(newGallery, () =>
        {
            Debug.Log("Gallery 保存成功！");
            // 获取某个 Gallery
            SharedDataManager.GetGallery("gallery001", gallery =>
            {
                if (gallery != null)
                    Debug.Log("获取到 Gallery：" + gallery.GalleryName);
                else
                    Debug.Log("未找到指定 Gallery");
            }, error =>
            {
                Debug.LogError("获取 Gallery 出错：" + error.GenerateErrorReport());
            });
        }, error =>
        {
            Debug.LogError("保存 Gallery 出错：" + error.GenerateErrorReport());
        });

        SharedDataManager.GetPublicGalleryIDs(galleryIds =>
        {
            Debug.Log("Public Gallery IDs:");
            foreach (var id in galleryIds)
            {
                Debug.Log(id);
            }
        }, error =>
        {
            Debug.LogError("获取 Public Gallery IDs 出错: " + error.GenerateErrorReport());
        });



        // 保存1个 Canva 数据
        SharedDataManager.SaveCanva("canva001", "https://example.com/image.png", () =>
        {
            Debug.Log("Canva 保存成功！");
        }, error =>
        {
            Debug.LogError("保存 Canva 出错：" + error.GenerateErrorReport());
        });

        // 登录成功后创建共享组
        //CreateSharedGroup();
    }




    //// 创建共享组（Shared Group）
    //void CreateSharedGroup()
    //{
    //    var request = new CreateSharedGroupRequest
    //    {
    //        SharedGroupId = sharedGroupId
    //    };

    //    PlayFabClientAPI.CreateSharedGroup(request, OnCreateSharedGroupSuccess, OnError);
    //}

    //void OnCreateSharedGroupSuccess(CreateSharedGroupResult result)
    //{
    //    Debug.Log("共享组创建成功，Shared Group ID: " + sharedGroupId);
    //    // 创建成功后更新共享组数据
    //    UpdateSharedGroupData();
    //}

    //// 更新共享组数据，将 JSON 存入共享组
    //void UpdateSharedGroupData()
    //{
    //    var request = new UpdateSharedGroupDataRequest
    //    {
    //        SharedGroupId = sharedGroupId,
    //        Data = new Dictionary<string, string>
    //        {
    //            { "TestData", testDataJson }
    //        }
    //    };

    //    PlayFabClientAPI.UpdateSharedGroupData(request, OnUpdateSharedGroupDataSuccess, OnError);
    //}

    //void OnUpdateSharedGroupDataSuccess(UpdateSharedGroupDataResult result)
    //{
    //    Debug.Log("共享组数据更新成功");
    //    // 数据更新成功后，读取共享组数据
    //    GetSharedGroupData();
    //}

    //// 从共享组中读取数据
    //void GetSharedGroupData()
    //{
    //    var request = new GetSharedGroupDataRequest
    //    {
    //        SharedGroupId = sharedGroupId,
    //        // Keys 为 null 表示读取所有数据
    //        Keys = null
    //    };

    //    PlayFabClientAPI.GetSharedGroupData(request, OnGetSharedGroupDataSuccess, OnError);
    //}

    //void OnGetSharedGroupDataSuccess(GetSharedGroupDataResult result)
    //{
    //    if (result.Data != null && result.Data.ContainsKey("TestData"))
    //    {
    //        string data = result.Data["TestData"].Value;
    //        Debug.Log("读取到共享组数据: " + data);
    //    }
    //    else
    //    {
    //        Debug.Log("未找到共享组数据");
    //    }
    //}

    void OnError(PlayFabError error)
    {
        Debug.LogError("PlayFab 错误: " + error.GenerateErrorReport());
    }
}
