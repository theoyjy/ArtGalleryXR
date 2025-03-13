//using System;
//using PlayFab;
//using PlayFab.ClientModels;
//using UnityEngine;

//public class PlayFabHello : MonoBehaviour
//{
//    private string _customId = "TestPlayer";
//    private string _loginUserId = string.Empty;
//    private string _loginTime = string.Empty;
//    private string _itemInstanceId = string.Empty;

//    private string _entityId = string.Empty;
//    private string _entityType = string.Empty;

//    public void Start()
//    {
//        if (string.IsNullOrEmpty(PlayFabSettings.staticSettings.TitleId))
//        {
//            PlayFabSettings.staticSettings.TitleId = "D41B8";
//        }

//        var request = new LoginWithCustomIDRequest
//        {
//            CustomId = _customId,
//            CreateAccount = true
//        };

//        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
//    }

//    public void Update()
//    {
//        if (Input.GetKeyDown(KeyCode.Space))
//        {
//            PlayFabUsage.GetTitleData();
//        }

//        if (Input.GetKeyDown(KeyCode.M))
//        {
//            if (string.IsNullOrEmpty(_loginUserId) || string.IsNullOrEmpty(_loginTime))
//            {
//                Debug.LogError("Please log in first");
//                return;
//            }

//            PlayFabUsage.UpdateUserDataRequest(_loginUserId, _loginTime);
//        }

//        if (Input.GetKeyDown(KeyCode.C))
//        {
//            PlayFabUsage.GetUserDataRequest();
//        }

//        if (Input.GetKeyDown(KeyCode.D))
//        {
//            if (string.IsNullOrEmpty(_entityId) || string.IsNullOrEmpty(_entityType))
//            {
//                Debug.LogError("Please log in first");
//                return;
//            }

//            PlayFabUsage.SetEntityObject(_entityId, _entityType);
//        }

//        if (Input.GetKeyDown(KeyCode.E))
//        {
//            if (string.IsNullOrEmpty(_entityId) || string.IsNullOrEmpty(_entityType))
//            {
//                Debug.LogError("Please log in first");
//                return;
//            }

//            PlayFabUsage.GetEntityObject(_entityId, _entityType);
//        }
//    }

//    private void OnLoginSuccess(LoginResult result)
//    {
//        Debug.Log($"Hello PlayFab£¡ {_customId}");

//        _loginUserId = result.PlayFabId;
//        _loginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

//        _entityId = result.EntityToken.Entity.Id;
//        _entityType = result.EntityToken.Entity.Type;

//        PlayFabUsage.LoadOrCreateGallery(_entityId, _entityType, (galleryId, canvasId) =>
//        {
//            Debug.Log("Gallery ID: {galleryId}");
//            Debug.Log("Canvas ID: {canvasId}");
//        });
//    }

//    private void OnLoginFailure(PlayFabError error)
//    {
//        Debug.LogError(error.GenerateErrorReport());
//    }
//}
