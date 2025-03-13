//using System;
//using System.Collections.Generic;
//using PlayFab;
//using PlayFab.ClientModels;
//using PlayFab.DataModels;
//using UnityEngine;

//public class PlayFabUsage : MonoBehaviour
//{
//    #region UseGameData£¨TitleData£©

//    public static void GetTitleData()
//    {
//        var request = new PlayFab.ClientModels.GetTitleDataRequest();
//        PlayFab.PlayFabClientAPI.GetTitleData(request, result =>
//        {
//            foreach (var dataPair in result.Data)
//            {
//                Debug.Log($"GetTitleData == {dataPair.Key} == {dataPair.Value}");
//            }
//        },
//            error =>
//            {
//                Debug.LogError($"GetTitleData == {error.GenerateErrorReport()}");
//            }
//        );
//    }

//    #endregion

//    #region UseUserData£¨UserData£©

//    public static void UpdateUserDataRequest(string userId, string loginTime)
//    {
//        var updateUserDataRequest = new PlayFab.ClientModels.UpdateUserDataRequest
//        {
//            Data = new Dictionary<string, string>
//            {
//                {"UserId", userId},
//                {"LoginTime", loginTime}
//            }
//        };

//        PlayFab.PlayFabClientAPI.UpdateUserData(updateUserDataRequest, result =>
//        {
//            Debug.Log($"UpdateUserDataRequest == Success {result.DataVersion}");
//        },
//            error =>
//            {
//                Debug.LogError($"UpdateUserDataRequest == {error.GenerateErrorReport()}");
//            }
//        );
//    }

//    [Serializable]
//    private class GalleryData
//    {
//        public string galleryId;
//        public string canvasId;
//        public string owner;
//        public string createdAt;
//    }

//    public static void GetUserDataRequest()
//    {
//        var request = new PlayFab.ClientModels.GetUserDataRequest();

//        PlayFab.PlayFabClientAPI.GetUserData(request, result =>
//        {
//            foreach (var dataPair in result.Data)
//            {
//                Debug.Log($"GetUserDataRequest == {dataPair.Key} == {dataPair.Value.Value}");
//            }
//        },
//            error =>
//            {
//                Debug.LogError($"GetUserDataRequest == {error.GenerateErrorReport()}");
//            }
//        );
//    }

//    #endregion

//    #region UseEntityData£¨EntityData£©

//    public static void SetEntityObject(string entityKey, string entityType)
//    {
//        var debugData = new Dictionary<string, object>()
//        {
//            { "Artist Achieve", true }
//        };
//        var dataList = new List<SetObject>()
//        {
//            new SetObject()
//            {
//                ObjectName = "MyPlayerData",
//                DataObject = debugData,
//            },
//        };

//        var newSetObjectRequest = new SetObjectsRequest()
//        {
//            Entity = new PlayFab.DataModels.EntityKey() { Id = entityKey, Type = entityType },
//            Objects = dataList,
//        };
//        PlayFabDataAPI.SetObjects(newSetObjectRequest, setResult =>
//        {
//            Debug.Log("SetEntityObject == Success with version {setResult.ProfileVersion}");
//        },
//            error =>
//            {
//                Debug.LogError(error.ErrorMessage);
//            });
//    }

//    public static void GetEntityObject(string entityKey, string entityType)
//    {
//        var newGetObjectRequest = new GetObjectsRequest()
//        {
//            Entity = new PlayFab.DataModels.EntityKey() { Id = entityKey, Type = entityType },
//        };
//        PlayFabDataAPI.GetObjects(newGetObjectRequest, getResult =>
//        {
//            Debug.Log("GetEntityObject == Success with version {getResult.ProfileVersion}");
//            foreach (var dataPair in getResult.Objects)
//            {
//                Debug.Log("GetEntityObject == {dataPair.Key} == {dataPair.Value.DataObject}");
//            }
//        },
//            error =>
//            {
//                Debug.LogError(error.ErrorMessage);
//            });
//    }

//    #endregion

//    private static void CreateGallery(string entityId, string entityType, Action<string, string> onGalleryCreated)
//    {
//        string galleryId = "GALLERY_" + Guid.NewGuid();
//        string canvasId = "CANVAS_" + Guid.NewGuid();

//        var galleryData = new GalleryData
//        {
//            galleryId = galleryId,
//            canvasId = canvasId,
//            owner = entityId,
//            createdAt = DateTime.UtcNow.ToString("o")
//        };

//        var request = new SetObjectsRequest
//        {
//            Entity = new PlayFab.DataModels.EntityKey { Id = entityId, Type = entityType },
//            Objects = new List<SetObject>
//        {
//            new SetObject { ObjectName = "GalleryData", DataObject = galleryData }
//        }
//        };

//        PlayFabDataAPI.SetObjects(request, result =>
//        {
//            Debug.Log("New Gallery created: {galleryId}, Canvas ID: {canvasId}");

//            onGalleryCreated?.Invoke(galleryId, canvasId);
//        },
//        error =>
//        {
//            Debug.LogError("Error creating gallery: {error.GenerateErrorReport()}");
//        });
//    }


//    public static void LoadOrCreateGallery(string entityId, string entityType, Action<string, string> onGalleryLoaded)
//    {
//        var request = new GetObjectsRequest
//        {
//            Entity = new PlayFab.DataModels.EntityKey { Id = entityId, Type = entityType }
//        };

//        PlayFabDataAPI.GetObjects(request, result =>
//        {
//            if (result.Objects != null && result.Objects.ContainsKey("GalleryData"))
//            {
//                var galleryData = JsonUtility.FromJson<GalleryData>(result.Objects["GalleryData"].DataObject.ToString());
//                Debug.Log("Gallery loaded: {galleryData.galleryId}, Canvas ID: {galleryData.canvasId}");

//                onGalleryLoaded?.Invoke(galleryData.galleryId, galleryData.canvasId);
//            }
//            else
//            {
//                CreateGallery(entityId, entityType, onGalleryLoaded);
//            }
//        },
//        error =>
//        {
//            Debug.LogError("Error fetching gallery: {error.GenerateErrorReport()}");
//        });
//    }

//    #region User Authentication (Login/Registration)

//    public static void LoginWithUsername(string username, string password, Action<LoginResult> onSuccess, Action<PlayFabError> onError)
//    {
//        var request = new LoginWithPlayFabRequest
//        {
//            Username = username,
//            Password = password
//        };
//        PlayFabClientAPI.LoginWithPlayFab(request, onSuccess, onError);
//    }

//    public static void RegisterNewAccount(string username, string password, string email, Action<RegisterPlayFabUserResult> onSuccess, Action<PlayFabError> onError)
//    {
//        var request = new RegisterPlayFabUserRequest
//        {
//            Username = username,
//            Password = password,
//            Email = email
//        };
//        PlayFabClientAPI.RegisterPlayFabUser(request, onSuccess, onError);
//    }

//    #endregion

//    #region User Data Management

//    public static void GetUserData(Action<GetUserDataResult> onSuccess, Action<PlayFabError> onError)
//    {
//        var request = new GetUserDataRequest();
//        PlayFabClientAPI.GetUserData(request, onSuccess, onError);
//    }

//    #endregion
//}