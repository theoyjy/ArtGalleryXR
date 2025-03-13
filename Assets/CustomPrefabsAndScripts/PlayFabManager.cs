using System;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;

public class PlayFabManager : MonoBehaviour
{
    [Header("UI")]
    public InputField userIDInput;
    public InputField passwordInput;
    public Text messageText;

    // Register & Login
    public void RegisterAndLoginButton()
    {
        string username = userIDInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            messageText.text = "ID or Password Can Not Be Empty!";
            return;
        }

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

            // Login
            Login(username, password);

        }, error =>
        {
            messageText.text = "Registration failed: " + error.ErrorMessage;
            Debug.LogError("Registration failed: " + error.GenerateErrorReport());
        });
    }

    // Login Button
    public void LoginButton()
    {
        string username = userIDInput.text;
        string password = passwordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            messageText.text = "ID or Password Can Not Be Empty!";
            return;
        }

        Login(username, password);
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

            // ��¼�ɹ���Ĳ���������ת������ʼ���û�����
            // LoadUserGallery(result.PlayFabId); // ����������Ҫ���� gallery���������������

        }, error =>
        {
            messageText.text = "Login Failed: " + error.ErrorMessage;
            Debug.LogError("Login Failed: " + error.GenerateErrorReport());
        });
    }
}
