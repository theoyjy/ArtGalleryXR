using System;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using Unity.Services.Lobbies.Models;

/// <summary>
/// 表示共享组的状态
/// </summary>
public enum SharedGroupState
{
    NotCreated,       // 共享组不存在
    CreatedEmpty,     // 共享组已存在，但没有任何数据
    CreatedWithData   // 共享组已存在且包含数据
}

[Serializable]
public class GalleryDetail
{
    public string GalleryID;           // 画廊ID
    public string GalleryName;         // 画廊名称
    public List<string> Canva;         // 包含所有 Canva ID 的列表
    public string OwnID;
    public string permission;          // 权限，仅有 "public" 或 "private"
}

public static class SharedDataManager
{
    // 两个模块的共享组 ID
    public static readonly string GallerySharedGroupId = "Galleries";
    public static readonly string CanvaSharedGroupId = "Canvas";
    public static readonly string PlayerSharedGroupId = "Players";

    public static string CurrentUserName;
    public static Lobby CurrentLobby;

    [Serializable]
    public class CloudScriptResponse
    {
        public string message;
        // 如果你还想用 result，可以加上
        public object result;
    }

    #region Shared Group Modify(Need cloud script)

    /// <summary>
    /// 使用 CloudScript 更新 Gallery 数据。
    /// 修改后：先检查共享组是否存在，不存在则创建，然后再调用CloudScript保存数据。
    /// </summary>
    public static void SaveGalleryUsingCloudScript(GalleryDetail gallery, Action onSuccess, Action<PlayFabError> onError)
    {
        // 第一步：先判断共享组是否存在
        var checkGroupRequest = new GetSharedGroupDataRequest
        {
            SharedGroupId = GallerySharedGroupId,
            Keys = null
        };

        PlayFabClientAPI.GetSharedGroupData(checkGroupRequest,
        groupResult =>
        {
        // 若成功获取数据（即使为空），表示共享组存在，可直接调用CloudScript更新数据
        ExecuteUpdateGalleryCloudScript(gallery, onSuccess, onError);
        },
        error =>
        {
            if (error.ErrorMessage.Contains("Shared group does not exist"))
            {
            // 第二步：如果共享组不存在则创建
            PlayFabClientAPI.CreateSharedGroup(new CreateSharedGroupRequest
                {
                    SharedGroupId = GallerySharedGroupId
                }, createResult =>
                {
                    Debug.Log("已成功创建共享组: " + GallerySharedGroupId);
                // 创建成功后，调用CloudScript更新数据
                ExecuteUpdateGalleryCloudScript(gallery, onSuccess, onError);
                },
                createError =>
                {
                    Debug.LogError("创建共享组失败: " + createError.ErrorMessage);
                    onError?.Invoke(createError);
                });
            }
            else
            {
            // 若是其他错误则直接返回
            Debug.LogError("检查共享组存在失败: " + error.ErrorMessage);
                onError?.Invoke(error);
            }
        });
    }

    /// <summary>
    /// 实际调用CloudScript去更新Gallery数据的函数
    /// </summary>
    private static void ExecuteUpdateGalleryCloudScript(GalleryDetail gallery, Action onSuccess, Action<PlayFabError> onError)
    {
        string json = JsonUtility.ToJson(gallery);
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "updateSharedGroupData",
            FunctionParameter = new
            {
                sharedGroupId = GallerySharedGroupId,
                key = gallery.GalleryID,
                value = json,
                permission = "Public"
            },
            GeneratePlayStreamEvent = true
        };

        PlayFabClientAPI.ExecuteCloudScript(request, result =>
        {
            if (result.FunctionResult != null)
            {
                var responseJson = result.FunctionResult.ToString();
                var response = JsonUtility.FromJson<CloudScriptResponse>(responseJson);
                Debug.Log("CloudScript message: " + response.message);
            }
            else
            {
                Debug.LogWarning("CloudScript返回为空");
            }

            onSuccess?.Invoke();
        },
        error =>
        {
            Debug.LogError("CloudScript 执行出错: " + error.ErrorMessage);
            onError?.Invoke(error);
        });
    }


    /// <summary>
    /// 使用 CloudScript 更新 Canva 数据。
    /// </summary>
    public static void SaveCanvaUsingCloudScript(string canvaID, string imageUrl, Action onSuccess, Action<PlayFabError> onError)
    { 
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "updateSharedGroupData",
            FunctionParameter = new
            {
                sharedGroupId = CanvaSharedGroupId,
                key = canvaID,
                value = imageUrl,
                permission = "Public"
            },
            GeneratePlayStreamEvent = true
        };

        PlayFabClientAPI.ExecuteCloudScript(request, result => {
            Debug.Log("CloudScript 执行成功: " + result.FunctionResult);
            onSuccess?.Invoke();
        }, error => {
            Debug.LogError("CloudScript 执行出错: " + error.ErrorMessage);
            onError?.Invoke(error);
        });
    }

    public static void CreateGallery(string GalleryID, string GalleryName,bool IsPublic)
    {
        GalleryDetail NewGallery = new GalleryDetail
        {
            GalleryID = GalleryID,
            GalleryName = GalleryName,
            Canva = new List<string> {},
            OwnID = PlayFabManager.CurrentUsername,
            permission = IsPublic ? "public" : "private"
        };

        SharedDataManager.SaveGalleryUsingCloudScript(NewGallery,
        onSuccess: () =>
        {
            Debug.Log("CloudScript 测试：Gallery 数据保存成功！");
        },
        onError: (error) =>
        {
            Debug.LogError("CloudScript 测试：Gallery 数据保存失败！");
        }
        );
    }


    #endregion




    #region GET PART(DON'T NEED CLOUD SCRIPT)
    /// <summary>
    /// 获取指定 Gallery 数据。如果共享组不存在则直接报错。
    /// </summary>
    public static void GetGallery(string galleryID, Action<GalleryDetail> onSuccess, Action<PlayFabError> onError)
    {
        var request = new GetSharedGroupDataRequest
        {
            SharedGroupId = GallerySharedGroupId,
            Keys = new List<string> { galleryID }
        };

        PlayFabClientAPI.GetSharedGroupData(request, result =>
        {
            if (result.Data != null && result.Data.ContainsKey(galleryID))
            {
                string json = result.Data[galleryID].Value;
                GalleryDetail gallery = JsonUtility.FromJson<GalleryDetail>(json);
                onSuccess?.Invoke(gallery);
            }
            else
            {
                // 如果获取不到数据，则认为共享组数据为空或组不存在，直接返回错误
                onError?.Invoke(new PlayFabError { ErrorMessage = "未找到指定 Gallery 数据或共享组不存在" });
            }
        }, onError);
    }

    #endregion

    #region Canva 模块
    /// <summary>
    /// 获取指定 Canva 的图片地址。如果共享组不存在则直接报错。
    /// </summary>
    public static void GetCanva(string canvaID, Action<string> onSuccess, Action<PlayFabError> onError)
    {
        var request = new GetSharedGroupDataRequest
        {
            SharedGroupId = CanvaSharedGroupId,
            Keys = new List<string> { canvaID }
        };

        PlayFabClientAPI.GetSharedGroupData(request, result =>
        {
            if (result.Data != null && result.Data.ContainsKey(canvaID))
            {
                string url = result.Data[canvaID].Value;
                onSuccess?.Invoke(url);
            }
            else
            {
                onError?.Invoke(new PlayFabError {  ErrorMessage = "未找到指定 Canva 数据或共享组不存在" });
            }
        }, onError);
    }


    /// <summary>
    /// 获取所有 Gallery 数据。
    /// </summary>
    /// <param name="onSuccess">成功回调，返回 GalleryDetail 列表</param>
    /// <param name="onError">错误回调</param>
    public static void GetAllGalleries(Action<List<GalleryDetail>> onSuccess, Action<PlayFabError> onError)
    {
        var request = new GetSharedGroupDataRequest
        {
            SharedGroupId = GallerySharedGroupId,
            // Keys 为 null 表示读取所有数据
            Keys = null
        };

        PlayFabClientAPI.GetSharedGroupData(request, result =>
        {
            List<GalleryDetail> galleries = new List<GalleryDetail>();
            if (result.Data != null && result.Data.Count > 0)
            {
                foreach (var kvp in result.Data)
                {
                    // 将每个存储的 JSON 字符串反序列化为 GalleryDetail 对象
                    GalleryDetail gallery = JsonUtility.FromJson<GalleryDetail>(kvp.Value.Value);
                    galleries.Add(gallery);
                }
            }
            onSuccess?.Invoke(galleries);
        }, onError);
    }


    /// <summary>
    /// 获取所有 Gallery 数据中，permission 为 "public" 的 GalleryID 列表。
    /// </summary>
    /// <param name="onSuccess">成功回调，返回符合条件的 GalleryID 列表</param>
    /// <param name="onError">错误回调</param>
    public static void GetPublicGalleryIDs(Action<List<string>> onSuccess, Action<PlayFabError> onError)
    {
        GetAllGalleries(galleries =>
        {
            List<string> publicGalleryIDs = new List<string>();
            foreach (GalleryDetail gallery in galleries)
            {
                // 如果 permission 不为空且为 "public"（不区分大小写），则添加 GalleryID
                if (!string.IsNullOrEmpty(gallery.permission) &&
                    gallery.permission.Equals("public", StringComparison.OrdinalIgnoreCase))
                {
                    publicGalleryIDs.Add(gallery.GalleryID);
                }
            }
            onSuccess?.Invoke(publicGalleryIDs);
        }, onError);
    }



    /// <summary>
    /// 获取所有 Gallery 数据中，permission 为 "public" 的 GalleryID 列表。
    /// </summary>
    /// <param name="onSuccess">成功回调，返回符合条件的 GalleryID 列表</param>
    /// <param name="onError">错误回调</param>
    public static void GetPrivateGalleryIDs(Action<List<string>> onSuccess, Action<PlayFabError> onError)
    {
        GetAllGalleries(galleries =>
        {
            List<string> publicGalleryIDs = new List<string>();
            foreach (GalleryDetail gallery in galleries)
            {
                // 如果 permission 不为空且为 "private"（不区分大小写），则添加 GalleryID
                if (!string.IsNullOrEmpty(gallery.permission) &&
                    gallery.permission.Equals("private", StringComparison.OrdinalIgnoreCase))
                {
                    publicGalleryIDs.Add(gallery.GalleryID);
                }
            }
            onSuccess?.Invoke(publicGalleryIDs);
        }, onError);
    }

    #endregion
}
