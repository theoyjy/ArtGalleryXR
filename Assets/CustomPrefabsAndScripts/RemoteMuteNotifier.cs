using System;
using UnityEngine;

public class RemoteMuteNotifier : MonoBehaviour
{
    // Static event: subscriber callbacks receive the remote player's unique ID and the new mute state.
    public static event Action<string, bool> OnRemoteMuteChanged;

    /// <summary>
    /// Call this method to broadcast that the mute state of a player has changed.
    /// </summary>
    /// <param name="playerId">Unique identifier for the remote player.</param>
    /// <param name="isMuted">True if muted; false otherwise.</param>
    public static void NotifyRemoteMuteChanged(string playerId, bool isMuted)
    {
        OnRemoteMuteChanged?.Invoke(playerId, isMuted);
    }
}
