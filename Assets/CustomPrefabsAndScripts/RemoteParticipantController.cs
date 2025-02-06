using UnityEngine;
using UnityEngine.UI;

public class RemoteParticipantController : MonoBehaviour
{
    [Header("Player Settings")]
    [Tooltip("A unique identifier for this remote player (set via your networking solution).")]
    public string playerId;

    [Header("UI Elements")]
    [Tooltip("UI Image representing the speaker icon above the player.")]
    public Image speakerIcon;
    [Tooltip("Sprite to display when the player is unmuted.")]
    public Sprite unmutedIcon;
    [Tooltip("Sprite to display when the player is muted.")]
    public Sprite mutedIcon;

    [Header("Audio")]
    [Tooltip("AudioSource playing this remote participant's voice.")]
    public AudioSource audioSource;

    // Tracks the local mute state for this remote participant.
    private bool isMuted = false;

    void Start()
    {
        // Ensure UI reflects the initial state.
        UpdateUI();

        // Subscribe to remote mute events.
        RemoteMuteNotifier.OnRemoteMuteChanged += OnRemoteMuteChanged;
    }

    void OnDestroy()
    {
        RemoteMuteNotifier.OnRemoteMuteChanged -= OnRemoteMuteChanged;
    }

    /// <summary>
    /// Called when the local user clicks the mute button for this remote participant.
    /// This toggles the mute state, mutes/unmutes the audio source, and updates the UI.
    /// </summary>
    public void ToggleMute()
    {
        isMuted = !isMuted;

        if (audioSource != null)
        {
            // Mute the AudioSource so you no longer hear them.
            audioSource.mute = isMuted;
        }

        UpdateUI();

        // Broadcast this change so any other systems can update accordingly.
        RemoteMuteNotifier.NotifyRemoteMuteChanged(playerId, isMuted);
    }

    /// <summary>
    /// Allows external calls to set the mute state directly.
    /// </summary>
    public void SetMute(bool mute)
    {
        isMuted = mute;
        if (audioSource != null)
        {
            audioSource.mute = isMuted;
        }
        UpdateUI();
    }

    /// <summary>
    /// Callback for when a remote mute state change is broadcast.
    /// Only processes the event if the changed player's ID matches this one.
    /// </summary>
    void OnRemoteMuteChanged(string changedPlayerId, bool muted)
    {
        if (changedPlayerId == playerId)
        {
            SetMute(muted);
            Debug.Log("Remote participant " + playerId + " mute state updated: " + (isMuted ? "Muted" : "Unmuted"));
        }
    }

    /// <summary>
    /// Updates the speaker icon based on the current mute state.
    /// </summary>
    void UpdateUI()
    {
        if (speakerIcon != null)
        {
            speakerIcon.sprite = isMuted ? mutedIcon : unmutedIcon;
        }
        else
        {
            Debug.LogWarning("Speaker icon reference is missing on player " + playerId);
        }
    }
}
