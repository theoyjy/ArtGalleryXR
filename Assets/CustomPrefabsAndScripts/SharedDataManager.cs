using System;
using System.Collections.Generic;
using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using Unity.Services.Lobbies.Models;



//

[Serializable]
public class GalleryDetail
{
    public string GalleryID;           // GalleryID
    public string LobbyID;         // LobbyID
    public List<string> Canvas;         // include Canva ID's URL 3.28changing
    public string OwnID;
    public string permission;          // 权限，仅有 "public" 或 "private"
}

public static class SharedDataManager
{
    // 两个模块的共享组 ID
    public static readonly string GallerySharedGroupId = "Gallery";
    public static readonly string PlayerSharedGroupId = "Players";

    public static string CurrentUserName;
    public static Lobby CurrentLobby;


    [Serializable]
    public class CloudScriptResponse
    {
        public string message;
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


    public static void CreateGallery(string GalleryID,string LobbyID, bool IsPublic)
    {
        GetAllGalleries(existingGalleries =>
        {
            GalleryDetail NewGallery = new GalleryDetail
            {
                GalleryID = GalleryID,
                LobbyID = LobbyID,
                Canvas = new List<string>(),
                OwnID = CurrentUserName,
                permission = IsPublic ? "public" : "private"
            };

            SaveGalleryUsingCloudScript(NewGallery,
            onSuccess: () =>
            {
                Debug.Log($"CloudScript 测试：Gallery {GalleryID} 数据保存成功！");
            },
            onError: (error) =>
            {
                Debug.LogError($"CloudScript 测试：Gallery {GalleryID} 数据保存失败！");
            });
        },
        onError: error =>
        {
            Debug.LogError("获取已有Gallery失败：" + error.ErrorMessage);
        });
    }

    //change lobby ID
    public static void ChangeLobbyID(string GalleryID, string LobbyID)
    {
        // 先尝试获取指定的 Gallery
        GetGallery(GalleryID, gallery =>
        {
            // 如果获取成功，则更新 LobbyID
            gallery.LobbyID = LobbyID;

            // 接着保存更新后的数据
            SaveGalleryUsingCloudScript(gallery,
                onSuccess: () =>
                {
                    Debug.Log($"成功更新 Gallery [{GalleryID}] 的 LobbyID 为: {LobbyID}");
                },
                onError: (error) =>
                {
                    Debug.LogError($"更新 Gallery [{GalleryID}] LobbyID 失败: {error.ErrorMessage}");
                });
        },
        error =>
        {
        // 如果在 GetGallery 时出现错误，说明该 Gallery 不存在或无法获取
        Debug.LogError($"Gallery [{GalleryID}] 不存在，无法更新 LobbyID。错误信息: {error.ErrorMessage}");
        });
    }



    /// <summary>
    /// 通用函数：创建共享数据（若共享组不存在则自动创建）
    /// </summary>
    private static void CreateSharedData(string sharedGroupId, string key, object value, Action onSuccess, Action<PlayFabError> onError)
    {
        var request = new GetSharedGroupDataRequest
        {
            SharedGroupId = sharedGroupId,
            Keys = null
        };

        PlayFabClientAPI.GetSharedGroupData(request, result =>
        {
            ExecuteSaveUsingCloudScript(sharedGroupId, key, value, onSuccess, onError);
        },
        error =>
        {
            if (error.ErrorMessage.Contains("Shared group does not exist"))
            {
                PlayFabClientAPI.CreateSharedGroup(new CreateSharedGroupRequest
                {
                    SharedGroupId = sharedGroupId
                }, createResult =>
                {
                    Debug.Log($"成功创建共享组：{sharedGroupId}");
                    ExecuteSaveUsingCloudScript(sharedGroupId, key, value, onSuccess, onError);
                }, createError =>
                {
                    Debug.LogError($"创建共享组 {sharedGroupId} 失败: {createError.ErrorMessage}");
                    onError?.Invoke(createError);
                });
            }
            else
            {
                Debug.LogError($"检查共享组 {sharedGroupId} 失败: {error.ErrorMessage}");
                onError?.Invoke(error);
            }
        });
    }

    /// <summary>
    /// 执行 CloudScript 保存数据
    /// </summary>
    private static void ExecuteSaveUsingCloudScript(string sharedGroupId, string key, object value, Action onSuccess, Action<PlayFabError> onError)
    {
        string json = value is string ? value.ToString() : JsonUtility.ToJson(value);
        var request = new ExecuteCloudScriptRequest
        {
            FunctionName = "updateSharedGroupData",
            FunctionParameter = new
            {
                sharedGroupId = sharedGroupId,
                key = key,
                value = json,
                permission = "Public"
            },
            GeneratePlayStreamEvent = true
        };

        PlayFabClientAPI.ExecuteCloudScript(request, result =>
        {
            Debug.Log("CloudScript 执行成功: " + result.FunctionResult);
            onSuccess?.Invoke();
        }, error =>
        {
            Debug.LogError("CloudScript 执行出错: " + error.ErrorMessage);
            onError?.Invoke(error);
        });
    }



    /// <summary>
    /// 创建 Player 数据
    /// </summary>
    public static void CreatePlayer(string playerID, List<string> galleryIDs)
    {
        CreateSharedData(PlayerSharedGroupId, playerID, galleryIDs,
        () => Debug.Log($"Player {playerID} 保存成功！"),
        error => Debug.LogError($"保存 Player {playerID} 失败: {error.ErrorMessage}"));
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
    //add canvas automatically to the empty one
    /// 向指定gallery添加canvas，自动寻找空的slot
    /// </summary>
    public static void AddCanva(string GalleryID, string canvaURL, Action<string> onSuccess, Action<PlayFabError> onError)
    {
        GetGallery(GalleryID, gallery =>
        {
            // 初始化 Canvas（若为空）
            if (gallery.Canvas == null)
            {
                gallery.Canvas = new List<string>(new string[40]);
            }
            else if (gallery.Canvas.Count < 40)
            {
                // 补全 Canvas 到 40 个 slot
                while (gallery.Canvas.Count < 40)
                {
                    gallery.Canvas.Add(string.Empty);
                }
            }

            // 找到第一个空的 slot
            int firstEmptyIndex = gallery.Canvas.IndexOf(string.Empty);
            if (firstEmptyIndex == -1)
            {
                // 若不存在空 slot，则返回
                Debug.LogError($"没有可用的空slot，画布已满: {GalleryID}");
                onSuccess?.Invoke("no_empty_slot");
                return;
            }

            // 将 URL 填入第一个空 slot
            gallery.Canvas[firstEmptyIndex] = canvaURL;

            // 保存更新
            SaveGalleryUsingCloudScript(gallery,
                () =>
                {
                    Debug.Log($"成功将 canvaURL 写入 gallery {GalleryID} 的 slot {firstEmptyIndex}");
                    onSuccess?.Invoke("success");
                },
                onError);
        },
        onError);
    }


    /// <summary>
    /// 向指定gallery添加canvas
    /// </summary>
    public static void SetCanva(string GalleryID, string canvaURL, int Slot, Action<string> onSuccess, Action<PlayFabError> onError)
    {
        GetGallery(GalleryID, gallery =>
        {
            // 初始化 Canvas（若为空）
            if (gallery.Canvas == null)
            {
                gallery.Canvas = new List<string>(new string[40]);
            }
            else if (gallery.Canvas.Count < 40)
            {
                // 补全 Canvas 到 40 个 slot
                while (gallery.Canvas.Count < 40)
                {
                    gallery.Canvas.Add(string.Empty);
                }
            }

            // 检查 slot 合法性
            if (Slot < 0 || Slot >= 40)
            {
                Debug.LogError($"invalid slot : {Slot}, need to between 0 and 39.");
                onSuccess?.Invoke("invalid_slot");
                return;
            }

            // 设置指定 slot 的 URL
            gallery.Canvas[Slot] = canvaURL;

            // 保存更新
            SaveGalleryUsingCloudScript(gallery,
                () =>
                {
                    Debug.Log($"成功将 canvaURL 写入 gallery {GalleryID} 的 slot {Slot}");
                    onSuccess?.Invoke("success");
                },
                onError);

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
    public static void GetPublicGalleries(Action<List<GalleryDetail>> onSuccess, Action<PlayFabError> onError)
    {
        GetAllGalleries(galleries =>
        {
            List<GalleryDetail> publicGalleryDetails = new List<GalleryDetail>();
            foreach (GalleryDetail gallery in galleries)
            {
                // 如果 permission 不为空且为 "public"（不区分大小写），则添加 GalleryID
                if (!string.IsNullOrEmpty(gallery.permission) &&
                    gallery.permission.Equals("public", StringComparison.OrdinalIgnoreCase))
                {
                    publicGalleryDetails.Add(gallery);
                }
            }
            onSuccess?.Invoke(publicGalleryDetails);
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
