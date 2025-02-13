using UnityEngine;
using System.Collections.Generic;

public class PlayerRegistry : MonoBehaviour
{
    public static PlayerRegistry Instance { get; private set; }

    private Dictionary<string, PlayerState> _playerMap
        = new Dictionary<string, PlayerState>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RegisterPlayer(string deviceName, PlayerState ps)
    {
        _playerMap[deviceName] = ps;
        Debug.Log($"[PlayerRegistry] Registered device '{deviceName}'");
    }

    public PlayerState GetPlayer(string deviceName)
    {
        _playerMap.TryGetValue(deviceName, out var ps);
        return ps;
    }

    public List<string> GetAllUserNames()
    {
        return new List<string>(_playerMap.Keys);
    }
    public void RemovePlayer(string userName)
    {
        if (_playerMap.ContainsKey(userName))
        {
            _playerMap.Remove(userName);
        }
    }

}
