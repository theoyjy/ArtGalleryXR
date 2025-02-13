using Unity.Netcode;
using UnityEngine;
using System;

public class PlayerState : NetworkBehaviour
{
    // Use the custom struct MyStringWrapper
    public NetworkVariable<MyStringWrapper> PlayerName
        = new NetworkVariable<MyStringWrapper>(
            new MyStringWrapper { Value = "" },                 // initial/default value
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
          );
    private StatusBar statusBar;
    private bool isMuted = false;

    private void Awake()
    {
        // Finds StatusBar anywhere in the child's hierarchy
        statusBar = GetComponentInChildren<StatusBar>();
    }


    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        Debug.Log($"OnNetworkSpawn - OwnerClientId={OwnerClientId}, " +
                  $"LocalClientId={NetworkManager.Singleton.LocalClientId}, " +
                  $"IsOwner={IsOwner}, gameObject={name}");

        if (IsOwner)
        {
            // For example, store the machine name
            string deviceName = Environment.MachineName;
            var wrapper = new MyStringWrapper { Value = deviceName };

            // Assign to the NetworkVariable
            PlayerName.Value = wrapper;

            // Optionally register in your local dictionary
            PlayerRegistry.Instance.RegisterPlayer(deviceName, this);
           // Debug.Log($"New registered user is: {deviceName}");
        }
       

        // If the struct's .Value is already set (e.g., we joined late)
        if (!string.IsNullOrEmpty(PlayerName.Value.Value))
        {
            //Debug.Log($"[OnNetworkSpawn] Found existing name: {PlayerName.Value.Value}");
            // Possibly do something with it immediately (like display a UI label).
            PlayerRegistry.Instance.RegisterPlayer(PlayerName.Value.Value, this);
        }

        // Listen for *future* changes
        PlayerName.OnValueChanged += (oldVal, newVal) =>
        {
            //Debug.Log($"[OnValueChanged] Player name changed from '{oldVal.Value}' to '{newVal.Value}'");
            Debug.Log("Value Changed");
            // Update your dictionary or UI
            PlayerRegistry.Instance.RegisterPlayer(newVal.Value, this);
            PrintAllUsersInLobby();
        };
    }
    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        // If we have a valid name, remove it from PlayerRegistry
        var userName = PlayerName.Value.Value;
        if (!string.IsNullOrEmpty(userName))
        {
            PlayerRegistry.Instance.RemovePlayer(userName);
            Debug.Log($"[OnNetworkDespawn] Removed user: {userName} from registry");
        }
    }

    private void PrintAllUsersInLobby()
    {
        var allUsers = PlayerRegistry.Instance.GetAllUserNames();
        Debug.Log($"[PlayerState] Current users in the lobby ({allUsers.Count}):");
        foreach (var user in allUsers)
        {
            Debug.Log($" - {user}");
        }
    }
    public void SetMutedVisual(bool isMuted)
    {
        // Toggle or update a local "muted" icon
        Debug.Log($"[{Environment.MachineName}] SetMutedVisual({isMuted})");
        // e.g. iconSprite.SetActive(isMuted);
        statusBar.SetMuteState(isMuted);
    }
}
