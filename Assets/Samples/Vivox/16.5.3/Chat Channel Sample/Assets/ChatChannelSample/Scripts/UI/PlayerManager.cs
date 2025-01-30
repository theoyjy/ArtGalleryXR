using System.Collections.Generic;
using UnityEngine;


public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;

    private Dictionary<string, StatusBar> playerUIMap = new Dictionary<string, StatusBar>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    
    public void RegisterPlayer(string playerName, StatusBar statusBar)
    {
        if (!playerUIMap.ContainsKey(playerName))
        {
            playerUIMap[playerName] = statusBar;
        }
    }
    

    public void UpdateMuteState(string playerName, bool isMuted)
    {
        
        if (playerUIMap.TryGetValue(playerName, out StatusBar statusBar))
        {
            statusBar.SetMuteState(isMuted);
        }
        
    }

    public void test()
    {
        Debug.Log("Say something");
    }
}
