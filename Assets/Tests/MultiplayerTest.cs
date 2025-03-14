using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

public class MultiplayManagerTests
{
    [UnityTest]
    public IEnumerator JoinToServerTest()
    {
        var networkManagerObject = new GameObject("NetworkManager");
        var networkManager = networkManagerObject.AddComponent<NetworkManager>();
        networkManagerObject.AddComponent<UnityTransport>();
        var multiplayObject = new GameObject("MultiplayManager");
        var multiplayManager = multiplayObject.AddComponent<MultiplayManager>();
        multiplayManager.ipAddress = "127.0.0.1";
        multiplayManager.port = 7777;
        multiplayManager.JoinToServer();
        yield return new WaitForSeconds(1f);
        Assert.IsTrue(NetworkManager.Singleton.IsClient);
    }
}
