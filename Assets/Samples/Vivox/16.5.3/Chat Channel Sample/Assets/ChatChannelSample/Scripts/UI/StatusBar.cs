using UnityEngine;
using UnityEngine.UI;

public class StatusBar : MonoBehaviour
{
    private Camera _cam;
    [SerializeField] private Image loudSpeakerMute;
    [SerializeField] private Image loudSpeakerLoud;

    // 用于标识该 StatusBar 属于哪个玩家
    private string playerName;
    // 当前静音状态
    private bool isMuted;

    void Start()
    {
        _cam = Camera.main;
        // 初始时按当前 isMuted 状态更新图标
        SetMuteState(isMuted);
    }

    void Update()
    {
        if (_cam != null)
        {
            // 始终让该 UI 面向摄像机
            transform.rotation = Quaternion.LookRotation(transform.position - _cam.transform.position);
        }
    }

    /// <summary>
    /// 初始化该 StatusBar，将其绑定到指定玩家（DisplayName）。
    /// </summary>
    public void Setup(string name)
    {
        playerName = name;
        // 更新显示状态（如果之前状态已有设置）
        SetMuteState(isMuted);
    }

    /// <summary>
    /// 设置静音状态并更新显示的图片。
    /// </summary>
    public void SetMuteState(bool muteState)
    {
        isMuted = muteState;
        if (loudSpeakerMute != null && loudSpeakerLoud != null)
        {
            loudSpeakerMute.gameObject.SetActive(isMuted);
            loudSpeakerLoud.gameObject.SetActive(!isMuted);
        }
    }

    /// <summary>
    /// 返回该 StatusBar 所属玩家的姓名
    /// </summary>
    public string GetPlayerName()
    {
        return playerName;
    }
}
