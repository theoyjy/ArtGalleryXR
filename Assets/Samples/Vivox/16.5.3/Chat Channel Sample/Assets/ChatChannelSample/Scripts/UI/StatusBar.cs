using UnityEngine;
using UnityEngine.UI;

public class StatusBar : MonoBehaviour
{
    private Camera _cam;
    [SerializeField] private Image loudSpeakerMute;
    [SerializeField] private Image loudSpeakerLoud;

    // ���ڱ�ʶ�� StatusBar �����ĸ����
    private string playerName;
    // ��ǰ����״̬
    private bool isMuted;

    void Start()
    {
        _cam = Camera.main;
        // ��ʼʱ����ǰ isMuted ״̬����ͼ��
        SetMuteState(isMuted);
    }

    void Update()
    {
        if (_cam != null)
        {
            // ʼ���ø� UI ���������
            transform.rotation = Quaternion.LookRotation(transform.position - _cam.transform.position);
        }
    }

    /// <summary>
    /// ��ʼ���� StatusBar������󶨵�ָ����ң�DisplayName����
    /// </summary>
    public void Setup(string name)
    {
        playerName = name;
        // ������ʾ״̬�����֮ǰ״̬�������ã�
        SetMuteState(isMuted);
    }

    /// <summary>
    /// ���þ���״̬��������ʾ��ͼƬ��
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
    /// ���ظ� StatusBar ������ҵ�����
    /// </summary>
    public string GetPlayerName()
    {
        return playerName;
    }
}
