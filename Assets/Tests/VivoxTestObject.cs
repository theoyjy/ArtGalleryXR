using UnityEngine;
using System.Threading.Tasks;

public class TestVivoxManager : MonoBehaviour
{
    private async void Start()
    {
        // 1. Access the VivoxVoiceManager singleton.
        VivoxVoiceManager manager = VivoxVoiceManager.Instance;
        Debug.Log($"VivoxVoiceManager instance found: {manager.name}");

        // 2. (Optional) Provide a test player name.
        //    This is only relevant if you’re using Unity’s Authentication package 
        //    or you want a user name for demonstration.
        string testPlayerName = "TestUser123";

        // 3. Call InitializeAsync on the manager.
        //    This will sign in (if using Auth) or just ensure the service is ready.
        await manager.InitializeAsync(testPlayerName);

        Debug.Log("VivoxVoiceManager has been initialized.");

        // 4. Optional: You can add more tests here, such as:
        //    - Checking if VivoxService.Instance is not null
        //    - Attempting to join a channel if you have that logic implemented
    }
}
