using System;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using UnityEngine;

public class AuthenticationManager : MonoBehaviour
{
    public bool isSignedIn = false; // Flag to track if signing in is already in progress

    async void Start()
    {
        await InitializeAuthentication();
    }

    private async Task InitializeAuthentication()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Signed in successfully.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Sign-in failed: {e.Message}");
            await InitializeAuthentication();
        }

        isSignedIn = AuthenticationService.Instance.IsSignedIn;

        if (isSignedIn || AuthenticationService.Instance.IsAuthorized)
        {
            Debug.Log("Instance is signed in now.");
        } else {
            Debug.Log("Instance not signed in.");
        }
    }
}
