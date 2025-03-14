using UnityEngine;
using UnityEngine.TestTools;
using NUnit.Framework;
using System.Collections;
using System.Reflection;
using System.Threading.Tasks;

public class VivoxVoiceManagerTests
{
    
   
    [UnityTest]
    public IEnumerator SingletonTest()
    {
        var existingManager = Object.FindObjectOfType<VivoxVoiceManager>();
        if (existingManager != null)
        {
            Object.Destroy(existingManager.gameObject);
            yield return null; 
        }

        GameObject go1 = new GameObject("VivoxTest1");
        VivoxVoiceManager manager1 = go1.AddComponent<VivoxVoiceManager>();
        yield return null; 

        VivoxVoiceManager instance1 = VivoxVoiceManager.Instance;
        Assert.AreEqual(manager1, instance1, "Singleton instance should be equal to the first created manager.");

      
        GameObject go2 = new GameObject("VivoxTest2");
        VivoxVoiceManager manager2 = go2.AddComponent<VivoxVoiceManager>();
        yield return null; 

        Assert.IsTrue(manager2 == null || manager2.Equals(null),
            "Duplicate VivoxVoiceManager should have been destroyed.");
        yield break;
    }

   
    [UnityTest]
    public IEnumerator CheckManualCredentialsTest()
    {
        GameObject go = new GameObject("VivoxTestCredentials");
        VivoxVoiceManager manager = go.AddComponent<VivoxVoiceManager>();
        yield return null; 

        MethodInfo checkMethod = typeof(VivoxVoiceManager)
            .GetMethod("CheckManualCredentials", BindingFlags.Instance | BindingFlags.NonPublic);
        bool hasManualCredentials = (bool)checkMethod.Invoke(manager, null);

        Assert.IsFalse(hasManualCredentials, "Expected CheckManualCredentials to return false when no credentials are set.");
        yield break;
    }

    
    [UnityTest]
    public IEnumerator InitializeAsyncTest()
    {
        GameObject go = new GameObject("VivoxTestInitAsync");
        VivoxVoiceManager manager = go.AddComponent<VivoxVoiceManager>();
        yield return null; 

        Task initTask = manager.InitializeAsync("TestPlayer");
        yield return new WaitUntil(() => initTask.IsCompleted);

        Assert.IsTrue(initTask.IsCompletedSuccessfully, "InitializeAsync should complete successfully.");
        yield break;
    }
}
