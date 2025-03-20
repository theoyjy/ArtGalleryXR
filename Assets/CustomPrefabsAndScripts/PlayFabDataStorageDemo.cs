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

        string ownID = "TestPlayer001";
        string galleryName = "Test Gallery";
        bool isPublic = true;

        // 调用 CreateNewGallery 方法
        SharedDataManager.CreateNewGallery(ownID, galleryName, isPublic,
        onSuccess: () =>
        {
            Debug.Log("Gallery create success！");
        },
        onError: (error) =>
        {
            Debug.LogError("创建 Gallery 失败: " + error.ErrorMessage);
        });
}


    void OnError(PlayFabError error)
    {
        Debug.LogError("PlayFab 错误: " + error.GenerateErrorReport());
    }
}
