using System;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.XR;
using System.Collections.Generic;
using System.Collections;


public class PlayFabManager : MonoBehaviour
{
    [Header("UI")]
    public TMP_InputField userIDInput;
    public TMP_InputField passwordInput;
    public Text messageText;
    public GameObject loginUI;
    public GameObject functionButtons;
    public GameObject spinner;

    private bool isVRController;

    public static bool IsLoginActive = true;

    private void Start()
    {
        ShowLoginScreen(); // Block the inputs using deltaTime
        //Time.timeScale = 0; // Pause game
    }
    private void Update()
    {
        if (!isVRController)
        {
            if (XRSettings.enabled)
            {
                var inputDevices = new List<InputDevice>();
                InputDevices.GetDevicesWithCharacteristics(InputDeviceCharacteristics.Controller, inputDevices);

                if (inputDevices.Count > 0)
                {
                    Time.timeScale = 1;
                    isVRController = true;
                }
            }
        }
    }
    public void ButtonTest()
    {
        functionButtons.SetActive(false);
        spinner.SetActive(true);
    }

    private void ShowLoginScreen()
    {
        loginUI.SetActive(true); // Show login UI
        IsLoginActive = true;
        Cursor.lockState = CursorLockMode.None; // set cursor visible
        Cursor.visible = true;

        isVRController = false;
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
        messageText.text = "Register Button Pressed";
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
            StartCoroutine(Login(username, password));

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
        messageText.text = "Login Button Pressed";

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
        {
            messageText.text = "ID or Password Can Not Be Empty!";
            return;
        }
        StartCoroutine(Login(username, password));
    }

    private IEnumerator Login(string username, string password)
    {
        bool isLoginSuccessful = true;
        bool isDone = true;

        functionButtons.SetActive(false);
        spinner.SetActive(true);


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
            //HideLoginScreen();    
            isLoginSuccessful = true;
            isDone = true;

        }, error =>
        {
            messageText.text = "Login Failed: " + error.ErrorMessage;
            Debug.LogError("Login Failed: " + error.GenerateErrorReport());
            isLoginSuccessful = false;
            isDone = true;
        });

        while (!isDone)
        {
            yield return null;
        }
        //Debug.LogError("is down");
        // 等待0.5秒展示loading效果
        yield return new WaitForSeconds(1.5f);
        //loadingIcon.SetActive(false);

        if (isLoginSuccessful)
        {
            // 播放成功动画（你可以用Animator代替SetActive）
            //successIcon.SetActive(true);
            //yield return new WaitForSeconds(1.0f); // 播放打钩动画的时间
            HideLoginScreen(); // 进入游戏
        }
        else
        {
            //loginButton.interactable = true;
            functionButtons.SetActive(false);
            spinner.SetActive(true);
        }
    }
}