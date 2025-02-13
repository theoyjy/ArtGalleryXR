using UnityEngine;
using Unity.Services.Vivox;
using System.Collections;

// Make sure you can see RosterItem from this assembly
// (i.e., "using ChatChannelSample.Scripts.UI;" or whatever namespace RosterItem is in)
// If RosterItem is in the global namespace, no extra using is needed.

[RequireComponent(typeof(RosterItem))]
public class MuteEventBridge : MonoBehaviour
{
    private RosterItem m_RosterItem;
    private StatusBar m_statusBar;

    private bool m_IsSubscribed = false;
    private bool m_StopTrying = false;

    private void Awake()
    {
        m_RosterItem = GetComponent<RosterItem>();
    }

    private void OnEnable()
    {
        // Option A: Start a coroutine that waits for the Participant to be assigned
        StartCoroutine(WaitForParticipant());
    }

    private IEnumerator WaitForParticipant()
    {
        // Wait until the RosterItem's Participant is set by the Vivox code
        while (!m_StopTrying && (m_RosterItem.Participant == null))
        {
            yield return null; // wait one frame
        }

        if (!m_StopTrying && m_RosterItem.Participant != null && !m_IsSubscribed)
        {
            // Now we have a valid participant
            m_RosterItem.Participant.ParticipantMuteStateChanged += OnParticipantMuteStateChanged;
            m_IsSubscribed = true;
            Debug.Log("MuteEventBridge: Subscribed to participant's mute state changes.");
        }
        else
        {
            Debug.Log("MuteEventBridge: Participant was never assigned before we stopped trying.");
        }
    }

    private void OnDisable()
    {
        m_StopTrying = true;  // stop the coroutine if it's still running

        // Unsubscribe if we successfully subscribed
        if (m_IsSubscribed && m_RosterItem?.Participant != null)
        {
            m_RosterItem.Participant.ParticipantMuteStateChanged -= OnParticipantMuteStateChanged;
            m_IsSubscribed = false;
        }
    }

    private void OnParticipantMuteStateChanged()
    {
        bool isMuted = m_RosterItem.Participant.IsMuted;
        string userName = m_RosterItem.Participant.DisplayName;

        Debug.Log($"[MuteEventBridge] Triggered '{userName}' => isMuted: {isMuted}");

        // Now correlate with your netcode logic
        var player = PlayerRegistry.Instance.GetPlayer(userName);
        if (player != null)
        {
            Debug.Log($"Successfully get the participant '{userName}'");
            // Toggle in-game mute icon or effect
            player.SetMutedVisual(isMuted);
        }
    }
}
