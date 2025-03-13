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
    public GameObject loginUI;

    public static bool IsLoginActive = true;

    private void Start()
    {
        ShowLoginScreen(); // Block the inputs using deltaTime
    }

    private void ShowLoginScreen()
    {
        loginUI.SetActive(true); // Show login UI
        IsLoginActive = true;
        Cursor.lockState = CursorLockMode.None; // set cursor visible
        Cursor.visible = true;
        Time.timeScale = 0; // Pause game
    }

    private void HideLoginScreen()
    {
        loginUI.SetActive(false); // Hide login UI
        IsLoginActive = false;
        Cursor.lockState = CursorLockMode.Locked; // Hide cursor
        Cursor.visible = false;
        Time.timeScale = 1; // Play game
    }

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

            // login successful and play game
            HideLoginScreen();

        }, error =>
        {
            messageText.text = "Login Failed: " + error.ErrorMessage;
            Debug.LogError("Login Failed: " + error.GenerateErrorReport());
        });
    }
}
