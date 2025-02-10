using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Vivox;
using System.Linq;

public class RosterItem : MonoBehaviour
{
    // 关联的 Vivox 参与者
    public VivoxParticipant Participant;
    public Text PlayerNameText;

    // 用于显示聊天状态的图标（静音、说话、不说话）
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

    // 用于远程玩家，本地记录静音状态
    private bool remoteMuteState = false;
    // 用于更新玩家头顶图标的 StatusBar（位于玩家预制体上的 Canvas 中）
    private StatusBar statusBar;

    private void Start()
    {
        // 为静音按钮添加点击监听
        MuteButton.onClick.AddListener(ToggleMute);
    }

    /// <summary>
    /// 点击静音按钮时调用：切换静音状态，同时更新聊天图标和玩家头顶 StatusBar 图标。
    /// </summary>
    private void ToggleMute()
    {
        if (Participant == null)
        {
            Debug.LogError("Participant is null in ToggleMute. 请先调用 SetupRosterItem().");
            return;
        }

        if (Participant.IsSelf)
        {
            // 本地玩家调用 Vivox 的静音方法
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
        }
        else
        {
            // 远程玩家，本地记录静音状态进行切换
            remoteMuteState = !remoteMuteState;
            if (remoteMuteState)
            {
                Participant.MutePlayerLocally();
                MuteButton.image.color = k_MutedColor;
            }
            else
            {
                Participant.UnmutePlayerLocally();
                MuteButton.image.color = Color.white;
            }
        }

        // 更新聊天图标
        UpdateChatStateImage();

        // 更新对应玩家头顶的 StatusBar 图标
        if (statusBar != null)
        {
            bool muted = Participant.IsSelf ? Participant.IsMuted : remoteMuteState;
            statusBar.SetMuteState(muted);
        }
    }

    /// <summary>
    /// 根据静音及说话状态更新聊天图标。
    /// </summary>
    private void UpdateChatStateImage()
    {
        bool muted = Participant.IsSelf ? Participant.IsMuted : remoteMuteState;
        if (muted)
        {
            ChatStateImage.sprite = MutedImage;
        }
        else if (Participant.SpeechDetected)
        {
            ChatStateImage.sprite = SpeakingImage;
        }
        else
        {
            ChatStateImage.sprite = NotSpeakingImage;
        }
    }

    /// <summary>
    /// 初始化该 RosterItem：绑定参与者信息、查找对应的 StatusBar（头顶图标）以及订阅状态更新事件。
    /// </summary>
    public void SetupRosterItem(VivoxParticipant participant)
    {
        Participant = participant;
        PlayerNameText.text = Participant.DisplayName;
        UpdateChatStateImage();

        // 通过所有场景中的 StatusBar 查找与当前玩家名称匹配的那个
        statusBar = FindObjectsOfType<StatusBar>()
                      .FirstOrDefault(sb => sb.GetPlayerName() == Participant.DisplayName);
        if (statusBar == null)
        {
            Debug.LogError("找不到玩家 " + Participant.DisplayName + " 的 StatusBar。请检查该玩家预制体上 Canvas 中是否挂有 StatusBar 并调用了 Setup().");
        }
        else
        {
            // 如果找到，确保 StatusBar 使用正确的玩家名称，并按初始静音状态显示
            statusBar.Setup(Participant.DisplayName);
            statusBar.SetMuteState(Participant.IsMuted);
        }

        // 订阅状态变化事件，保证状态更新时聊天图标自动更新
        Participant.ParticipantMuteStateChanged += UpdateChatStateImage;
        Participant.ParticipantSpeechDetected += UpdateChatStateImage;

        // 本地玩家不显示音量滑条
        if (participant.IsSelf)
        {
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

        // 根据 AudioTapsManager 状态配置效果下拉列表
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
