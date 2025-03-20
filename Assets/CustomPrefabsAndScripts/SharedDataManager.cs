using System;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

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
    public static readonly string GallerySharedGroupId = "gallery";
    public static readonly string CanvaSharedGroupId = "Canva";

    #region Shared Group 状态及创建函数

    /// <summary>
    /// 获取共享组状态：
    /// NotCreated - 共享组不存在
    /// CreatedEmpty - 共享组存在但无数据
    /// CreatedWithData - 共享组存在且有数据
    /// </summary>
    public static void GetSharedGroupState(string sharedGroupId, Action<SharedGroupState> onSuccess, Action<PlayFabError> onError)
    {
        var request = new GetSharedGroupDataRequest
        {
            SharedGroupId = sharedGroupId,
            Keys = null // 读取所有数据
        };

        PlayFabClientAPI.GetSharedGroupData(request, result =>
        {
            if (result.Data == null || result.Data.Count == 0)
            {
                // 共享组存在但没有数据
                onSuccess?.Invoke(SharedGroupState.CreatedEmpty);
            }
            else
            {
                // 共享组存在且有数据
                onSuccess?.Invoke(SharedGroupState.CreatedWithData);
            }
        },
        error =>
        {
            // 如果错误码为 SharedGroupNotFound，说明组不存在
            onSuccess?.Invoke(SharedGroupState.NotCreated);
        });
    }


    /// <summary>
    /// 创建共享组。外部也可以直接调用此函数创建共享组。
    /// </summary>
    public static void CreateSharedGroup(string sharedGroupId, Action onSuccess, Action<PlayFabError> onError)
    {
        var request = new CreateSharedGroupRequest
        {
            SharedGroupId = sharedGroupId
        };

        PlayFabClientAPI.CreateSharedGroup(request, result =>
        {
            Debug.Log("Shared Group 创建成功: " + sharedGroupId);
            onSuccess?.Invoke();
        }, error =>
        {
            // 如果报错是 InvalidSharedGroupId，说明该ID已经存在
            if (error.Error == PlayFabErrorCode.InvalidSharedGroupId)
            {
                Debug.Log("共享组ID " + sharedGroupId + " 已经创建，不再重复创建。");
                onSuccess?.Invoke();
            }
            else
            {
                onError?.Invoke(error);
            }
        });
    }

    /// <summary>
    /// 确保共享组存在。如果不存在则自动创建。
    /// </summary>
    public static void EnsureSharedGroupExists(string sharedGroupId, Action onSuccess, Action<PlayFabError> onError)
    {
        GetSharedGroupState(sharedGroupId, state =>
        {
            if (state == SharedGroupState.NotCreated)
            {
                // 自动创建共享组
                CreateSharedGroup(sharedGroupId, onSuccess, onError);
            }
            else
            {
                // 已存在（无论是否有数据），直接返回成功
                onSuccess?.Invoke();
            }
        }, onError);
    }

    #endregion

    #region Gallery 模块

    /// <summary>
    /// 保存或更新某个 Gallery 数据。调用时会先确保共享组存在，
    /// 如果组不存在则自动创建，然后更新指定 Gallery 的数据，并设置为 Public。
    /// </summary>
    public static void SaveGallery(GalleryDetail gallery, Action onSuccess, Action<PlayFabError> onError)
    {
        EnsureSharedGroupExists(GallerySharedGroupId, () =>
        {
            string json = JsonUtility.ToJson(gallery);
            var request = new UpdateSharedGroupDataRequest
            {
                SharedGroupId = GallerySharedGroupId,
                Data = new Dictionary<string, string>
            {
                { gallery.GalleryID, json }
            },
                // 将权限设置为 Public
                Permission = UserDataPermission.Public
            };

            PlayFabClientAPI.UpdateSharedGroupData(request, result =>
            {
                Debug.Log($"Gallery 数据更新成功（Public），GalleryID: {gallery.GalleryID}");
                onSuccess?.Invoke();
            }, onError);
        }, onError);
    }

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
    /// 保存或更新某个 Canva 数据，确保共享组存在。
    /// </summary>
    public static void SaveCanva(string canvaID, string imageUrl, Action onSuccess, Action<PlayFabError> onError)
    {
        EnsureSharedGroupExists(CanvaSharedGroupId, () =>
        {
            var request = new UpdateSharedGroupDataRequest
            {
                SharedGroupId = CanvaSharedGroupId,
                Data = new Dictionary<string, string>
                {
                    { canvaID, imageUrl }
                },
                // 将权限设置为 Public
                Permission = UserDataPermission.Public
            };

            PlayFabClientAPI.UpdateSharedGroupData(request, result =>
            {
                Debug.Log("Canva 数据更新成功，CanvaID: " + canvaID);
                onSuccess?.Invoke();
            }, onError);
        }, onError);
    }

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
