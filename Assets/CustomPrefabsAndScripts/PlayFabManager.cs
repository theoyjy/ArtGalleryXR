using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System;
using System.Text;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;

public class PlayFabManager : MonoBehaviour
{
    [Header("UI")]
    public InputField userIDInput;
    public InputField passwordInput;
    public Text messageText;
    public GameObject loginUI;

    public static bool IsLoginActive = true;

    private void Start()
    {
        ShowLoginScreen();
    }

    private void ShowLoginScreen()
    {
        loginUI.SetActive(true);
        IsLoginActive = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0;
    }

    private void HideLoginScreen()
    {
        loginUI.SetActive(false);
        IsLoginActive = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Time.timeScale = 1;
    }

    public void RegisterAndLoginButton()
    {
        string username = userIDInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            messageText.text = "ID or Password Cannot Be Empty!";
            return;
        }

        CheckUsernameExists(username, (exists) =>
        {
            if (exists)
            {
                messageText.text = "Username already exists! Please choose another one.";
            }
            else
            {
                RegisterUser(username, password);
            }
        });
    }

    private void CheckUsernameExists(string username, Action<bool> callback)
    {
        StartCoroutine(CheckUsernameCoroutine(username, callback));
    }

    private IEnumerator CheckUsernameCoroutine(string username, Action<bool> callback)
    {
        string url = "http://localhost:3000/checkUsername";

        var jsonData = "{\"username\":\"" + username + "\"}";
        var request = new UnityWebRequest(url, "POST")
        {
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonData)),
            downloadHandler = new DownloadHandlerBuffer()
        };
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Error checking username: " + request.error);
            callback(false);
        }
        else
        {
            string responseText = request.downloadHandler.text;

            if (responseText.Contains("\"exists\":true"))
            {
                callback(true);
            }
            else
            {
                callback(false);
            }
        }
    }

    private void RegisterUser(string username, string password)
    {
        var request = new RegisterPlayFabUserRequest
        {
            Username = username,
            Password = password,
            RequireBothUsernameAndEmail = false
        };

        PlayFabClientAPI.RegisterPlayFabUser(request, result =>
        {
            messageText.text = "Registered and Logging in...";
            Debug.Log("Register Successful");

            Login(username, password);

        }, error =>
        {
            messageText.text = "Registration failed: " + error.ErrorMessage;
            Debug.Log("Registration failed: " + error.GenerateErrorReport());
        });
    }

    private void Login(string username, string password)
    {
        var request = new LoginWithPlayFabRequest
        {
            Username = username,
            Password = password
        };

        PlayFabClientAPI.LoginWithPlayFab(request, result =>
        {
            messageText.text = "Login Successful";
            Debug.Log("Login Successful");

            // 保存用户名之前，不隐藏界面
            SaveUsername(username, password);

            // 确保保存完成后再隐藏界面
            HideLoginScreen();

        }, error =>
        {
            messageText.text = "Login Failed: " + error.ErrorMessage;
            Debug.Log("Login Failed: " + error.GenerateErrorReport());
        });
    }


    private void SaveUsername(string username, string password)
    {
        StartCoroutine(SaveUsernameCoroutine(username, password));
    }

    private IEnumerator SaveUsernameCoroutine(string username, string password)
    {
        string url = "http://localhost:3000/saveUsername";
        var jsonData = "{\"username\":\"" + username + "\",\"password\":\"" + password + "\"}";
        var request = new UnityWebRequest(url, "POST")
        {
            uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonData)),
            downloadHandler = new DownloadHandlerBuffer()
        };
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to save username: " + request.error);
        }
        else
        {
            string responseText = request.downloadHandler.text;
            Debug.Log("Response from server: " + responseText);

            if (responseText.Contains("\"success\":true"))
            {
                Debug.Log("Username '{username}' saved successfully to Title Data!");
            }
            else
            {
                Debug.LogError("Failed to save username to Title Data. Check server logs for more details.");
            }
        }
    }


    private void LoadGalleryInfo()
    {
        Debug.Log("Ready to load gallery information here.");
    }
}
