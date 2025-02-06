//using UnityEngine;
//using UnityEngine.UI;
//using Unity.Services.Vivox;
//using System.Linq;

//public class RosterItem : MonoBehaviour
//{
//    // Player specific items.
//    public VivoxParticipant Participant;
//    public Text PlayerNameText;

//    public Image ChatStateImage;
//    public Sprite MutedImage;
//    public Sprite SpeakingImage;
//    public Sprite NotSpeakingImage;
//    public Slider ParticipantVolumeSlider;
//    public Button MuteButton;
//    public Dropdown EffectDropdown;

//    const float k_minSliderVolume = -50;
//    const float k_maxSliderVolume = 7;
//    readonly Color k_MutedColor = new Color(1, 0.624f, 0.624f, 1);

//    private void Start()
//    {
//        MuteButton.onClick.AddListener(ToggleMute);
//    }

//    private void ToggleMute()
//    {
//        if (Participant.IsMuted)
//        {
//            Participant.UnmutePlayerLocally();
//        }
//        else
//        {
//            Participant.MutePlayerLocally();
//        }

//        // Notify the player UI
//        PlayerManager.Instance.UpdateMuteState(Participant.DisplayName, Participant.IsMuted);
//        UpdateChatStateImage();

//        if (Participant.IsSelf)
//        {
//            StatusBar.Instance.SetMuteState(Participant.IsMuted);
//        }

//    }
//    private void UpdateChatStateImage()
//    {
//        if (Participant.IsMuted)
//        {
//            ChatStateImage.sprite = MutedImage;
//            ChatStateImage.gameObject.transform.localScale = Vector3.one;
//        }
//        else
//        {
//            if (Participant.SpeechDetected)
//            {
//                ChatStateImage.sprite = SpeakingImage;
//                ChatStateImage.gameObject.transform.localScale = Vector3.one;
//            }
//            else
//            {
//                ChatStateImage.sprite = NotSpeakingImage;
//            }
//        }
//    }

//    public void SetupRosterItem(VivoxParticipant participant)
//    {
//        Participant = participant;
//        PlayerNameText.text = Participant.DisplayName;
//        UpdateChatStateImage();
//        Participant.ParticipantMuteStateChanged += UpdateChatStateImage;
//        Participant.ParticipantSpeechDetected += UpdateChatStateImage;

//        MuteButton.onClick.AddListener(() =>
//        {
//            // If already muted, unmute, and vice versa.
//            if (Participant.IsMuted)
//            {
//                participant.UnmutePlayerLocally();
//                MuteButton.image.color = Color.white;
//            }
//            else
//            {
//                participant.MutePlayerLocally();
//                MuteButton.image.color = k_MutedColor;
//            }
//        });

//        if (participant.IsSelf)
//        {
//            // Can't change our own participant volume, so turn off the slider
//            ParticipantVolumeSlider.gameObject.SetActive(false);
//        }
//        else
//        {
//            ParticipantVolumeSlider.minValue = k_minSliderVolume;
//            ParticipantVolumeSlider.maxValue = k_maxSliderVolume;
//            ParticipantVolumeSlider.value = participant.LocalVolume;
//            ParticipantVolumeSlider.onValueChanged.AddListener((val) =>
//            {
//                OnParticipantVolumeChanged(val);
//            });
//        }

//        EffectDropdown.gameObject.SetActive(AudioTapsManager.Instance.IsFeatureEnabled);

//        EffectDropdown.onValueChanged.AddListener(delegate
//        {
//            EffectChanged(EffectDropdown);
//        });
//        AudioTapsManager.Instance.OnTapsFeatureChanged += OnAudioTapsManagerFeatureChanged;
//    }

//    private void OnAudioTapsManagerFeatureChanged(bool enabled)
//    {
//        EffectDropdown.gameObject.SetActive(enabled);
//    }

//    private void EffectChanged(Dropdown effectDropDown)
//    {
//        var effect = (AudioTapsManager.Effects)effectDropDown.value;

//        if (Participant.IsSelf)
//        {
//            AudioTapsManager.Instance.AddSelfCaptureEffect(effect); // TODO: Re-enable transmit effects when CaptureSink becomes supported
//        }
//        else
//        {
//            AudioTapsManager.Instance.AddParticipantEffect(Participant, effect);
//        }
//    }

//    void OnDestroy()
//    {
//        Participant.ParticipantMuteStateChanged -= UpdateChatStateImage;
//        Participant.ParticipantSpeechDetected -= UpdateChatStateImage;
//        MuteButton.onClick.RemoveAllListeners();
//        ParticipantVolumeSlider.onValueChanged.RemoveAllListeners();
//    }

//    void OnParticipantVolumeChanged(float volume)
//    {
//        if (!Participant.IsSelf)
//        {
//            Participant.SetLocalVolume((int)volume);
//        }
//    }
//}

using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Vivox;
using System.Linq;

public class RosterItem : MonoBehaviour
{
    // Player-specific items.
    public VivoxParticipant Participant;
    public Text PlayerNameText;

    // Chat icon to show mute, speaking, or not-speaking status.
    public Image ChatStateImage;
    public Sprite MutedImage;
    public Sprite SpeakingImage;
    public Sprite NotSpeakingImage;

    public Slider ParticipantVolumeSlider;
    public Button MuteButton;
    public Dropdown EffectDropdown;

    const float k_minSliderVolume = -50;
    const float k_maxSliderVolume = 7;
    readonly Color k_MutedColor = new Color(1, 0.624f, 0.624f, 1);

    private void Start()
    {
        // *** FIX 1 & 4 ***  
        // Remove duplicate listeners: add one listener for mute toggling.
        MuteButton.onClick.AddListener(ToggleMute);
    }

    /// <summary>
    /// Called when the mute button is clicked.
    /// This toggles the mute state, updates the UI, and (if this is the local user)
    /// updates the global status bar.
    /// </summary>
    private void ToggleMute()
    {
        if (Participant == null)
        {
            Debug.LogError("Participant is null in RosterItem.ToggleMute. Make sure SetupRosterItem() was called.");
            return;
        }

        if (Participant.IsMuted)
        {
            Participant.UnmutePlayerLocally();
            MuteButton.image.color = Color.white;
        }
        else
        {
            Participant.MutePlayerLocally();
            MuteButton.image.color = k_MutedColor;
        }

        if (PlayerManager.Instance != null)
        {
            PlayerManager.Instance.UpdateMuteState(Participant.DisplayName, Participant.IsMuted);
        }
        else
        {
            Debug.LogWarning("PlayerManager.Instance is null.");
        }

        UpdateChatStateImage();

        if (Participant.IsSelf)
        {
            if (StatusBar.Instance != null)
            {
                StatusBar.Instance.SetMuteState(Participant.IsMuted);
            }
            else
            {
                Debug.LogWarning("StatusBar.Instance is null.");
            }
        }
    }


    /// <summary>
    /// Updates the chat icon based on whether the participant is muted or speaking.
    /// </summary>
    private void UpdateChatStateImage()
    {
        if (Participant.IsMuted)
        {
            ChatStateImage.sprite = MutedImage;
            ChatStateImage.gameObject.transform.localScale = Vector3.one;
        }
        else if (Participant.SpeechDetected)
        {
            ChatStateImage.sprite = SpeakingImage;
            ChatStateImage.gameObject.transform.localScale = Vector3.one;
        }
        else
        {
            ChatStateImage.sprite = NotSpeakingImage;
        }
    }

    public void SetupRosterItem(VivoxParticipant participant)
    {
        Participant = participant;
        PlayerNameText.text = Participant.DisplayName;
        UpdateChatStateImage();

        // Subscribe to events so that if the participant’s mute state or speaking state changes,
        // the roster item UI (icon) is automatically updated.
        Participant.ParticipantMuteStateChanged += UpdateChatStateImage;
        Participant.ParticipantSpeechDetected += UpdateChatStateImage;

        // *** FIX 1 & 4 ***  
        // Remove duplicate mute listeners. Do not add a second listener here.
        // (This prevents toggling twice and canceling out the mute state.)

        if (participant.IsSelf)
        {
            // Disable volume slider for yourself.
            ParticipantVolumeSlider.gameObject.SetActive(false);
        }
        else
        {
            ParticipantVolumeSlider.minValue = k_minSliderVolume;
            ParticipantVolumeSlider.maxValue = k_maxSliderVolume;
            ParticipantVolumeSlider.value = participant.LocalVolume;
            ParticipantVolumeSlider.onValueChanged.AddListener((val) =>
            {
                OnParticipantVolumeChanged(val);
            });
        }

        // Enable or disable the effect dropdown based on AudioTapsManager.
        EffectDropdown.gameObject.SetActive(AudioTapsManager.Instance.IsFeatureEnabled);
        EffectDropdown.onValueChanged.AddListener(delegate
        {
            EffectChanged(EffectDropdown);
        });
        AudioTapsManager.Instance.OnTapsFeatureChanged += OnAudioTapsManagerFeatureChanged;
    }

    private void OnAudioTapsManagerFeatureChanged(bool enabled)
    {
        EffectDropdown.gameObject.SetActive(enabled);
    }

    private void EffectChanged(Dropdown effectDropDown)
    {
        var effect = (AudioTapsManager.Effects)effectDropDown.value;

        if (Participant.IsSelf)
        {
            AudioTapsManager.Instance.AddSelfCaptureEffect(effect);
        }
        else
        {
            AudioTapsManager.Instance.AddParticipantEffect(Participant, effect);
        }
    }

    void OnDestroy()
    {
        if (Participant != null)
        {
            Participant.ParticipantMuteStateChanged -= UpdateChatStateImage;
            Participant.ParticipantSpeechDetected -= UpdateChatStateImage;
        }
        MuteButton.onClick.RemoveAllListeners();
        if (ParticipantVolumeSlider != null)
            ParticipantVolumeSlider.onValueChanged.RemoveAllListeners();
    }

    void OnParticipantVolumeChanged(float volume)
    {
        if (!Participant.IsSelf)
        {
            Participant.SetLocalVolume((int)volume);
        }
    }
}
