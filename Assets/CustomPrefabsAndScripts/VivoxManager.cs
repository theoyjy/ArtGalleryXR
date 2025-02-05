using System;
using UnityEngine;
using Unity.Services.Vivox; // Ensure the Vivox 16.5.3 SDK is imported

public class VivoxManager : MonoBehaviour
{
    // Singleton instance so other scripts can easily access the manager.
    public static VivoxManager Instance { get; private set; }

    // Public event to notify subscribers when the local mute state changes.
    public event Action<bool> OnLocalMuteStateChanged;

    // Private variable to track the current mute state.
    private bool isMuted = false;

    void Awake()
    {
        // Create the singleton instance.
        if (Instance == null)
        {
            Instance = this;
            // Optionally, make this object persist across scenes.
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Toggles the local mute state using Vivox's public API and notifies subscribers.
    /// </summary>
    public void ToggleLocalMute()
    {
        isMuted = !isMuted;

        if (isMuted)
        {
            VivoxService.Instance.MuteInputDevice();
            Debug.Log("Local input muted.");
        }
        else
        {
            VivoxService.Instance.UnmuteInputDevice();
            Debug.Log("Local input unmuted.");
        }

        // Notify all subscribers about the new mute state.
        OnLocalMuteStateChanged?.Invoke(isMuted);
    }

    /// <summary>
    /// Optionally, a method to set the mute state directly.
    /// </summary>
    public void SetLocalMute(bool mute)
    {
        if (mute != isMuted)
        {
            ToggleLocalMute();
        }
    }

    /// <summary>
    /// Returns the current mute state.
    /// </summary>
    public bool GetLocalMuteState()
    {
        return isMuted;
    }
}
