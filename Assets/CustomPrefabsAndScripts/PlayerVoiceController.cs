//using System;
//using UnityEngine;
//using UnityEngine.UI;
//using Unity.Services.Vivox; // Ensure the Vivox 16.5.3 SDK is imported

//public class PlayerVoiceController : MonoBehaviour
//{
//    [Header("UI Elements")]
//    [Tooltip("UI Image representing the speaker icon above the player.")]
//    public Image speakerIcon;

//    [Tooltip("Sprite to display when the player is muted.")]
//    public Sprite mutedIcon;

//    [Tooltip("Sprite to display when the player is unmuted.")]
//    public Sprite unmutedIcon;

//    // Tracks the local mute state.
//    private bool isMuted = false;

//    void Start()
//    {
//        // Set the initial speaker icon.
//        UpdateSpeakerIcon();

//        // (Optional) If you have another mechanism to detect external mute changes,
//        // subscribe to that event here.
//    }

//    void Update()
//    {
//        // Toggle mute state when the spacebar is pressed.
//        if (Input.GetKeyDown(KeyCode.Space))
//        {
//            ToggleMute();
//        }
//    }

//    /// <summary>
//    /// Toggles the local mute state and updates both the UI and Vivox.
//    /// </summary>
//    void ToggleMute()
//    {
//        isMuted = !isMuted;
//        UpdateSpeakerIcon();

//        // Use the public VivoxService API to mute/unmute the local input device.
//        if (isMuted)
//        {
//            VivoxService.Instance.MuteInputDevice();
//            Debug.Log("Player muted via spacebar toggle.");
//        }
//        else
//        {
//            VivoxService.Instance.UnmuteInputDevice();
//            Debug.Log("Player unmuted via spacebar toggle.");
//        }
//    }

//    /// <summary>
//    /// Updates the UI speaker icon based on the current mute state.
//    /// </summary>
//    void UpdateSpeakerIcon()
//    {
//        if (speakerIcon != null)
//        {
//            speakerIcon.sprite = isMuted ? mutedIcon : unmutedIcon;
//        }
//        else
//        {
//            Debug.LogWarning("Speaker icon reference is missing.");
//        }
//    }
//}


using UnityEngine;
using UnityEngine.UI;

public class PlayerVoiceController : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("UI Image representing the speaker icon above the player.")]
    public Image speakerIcon;

    [Tooltip("Sprite to display when the player is muted.")]
    public Sprite mutedIcon;

    [Tooltip("Sprite to display when the player is unmuted.")]
    public Sprite unmutedIcon;

    // Local variable to track mute state.
    private bool isMuted = false;

    void Start()
    {
        // Update the UI to the initial mute state.
        UpdateSpeakerIcon();

        // Subscribe to the mute state changed event from the VivoxManager.
        if (VivoxManager.Instance != null)
        {
            VivoxManager.Instance.OnLocalMuteStateChanged += OnLocalMuteStateChanged;
        }
        else
        {
            Debug.LogWarning("VivoxManager instance not found.");
        }
    }

    void OnDestroy()
    {
        // Unsubscribe from the event to avoid memory leaks.
        if (VivoxManager.Instance != null)
        {
            VivoxManager.Instance.OnLocalMuteStateChanged -= OnLocalMuteStateChanged;
        }
    }

    void Update()
    {
        // Listen for spacebar input to toggle mute.
        if (Input.GetKeyDown(KeyCode.Space))
        {
            VivoxManager.Instance.ToggleLocalMute();
        }
    }

    /// <summary>
    /// Callback that updates the UI when the mute state changes.
    /// </summary>
    /// <param name="muted">The new mute state.</param>
    void OnLocalMuteStateChanged(bool muted)
    {
        isMuted = muted;
        UpdateSpeakerIcon();
        Debug.Log("Mute state changed: " + (isMuted ? "Muted" : "Unmuted"));
    }

    /// <summary>
    /// Updates the speaker icon based on the current mute state.
    /// </summary>
    void UpdateSpeakerIcon()
    {
        if (speakerIcon != null)
        {
            speakerIcon.sprite = isMuted ? mutedIcon : unmutedIcon;
        }
        else
        {
            Debug.LogWarning("Speaker icon reference is missing.");
        }
    }
}
