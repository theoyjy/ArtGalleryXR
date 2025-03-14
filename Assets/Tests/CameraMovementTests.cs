#if !UNITY_NETCODE_PRESENT
namespace Unity.Netcode
{
    public class NetworkBehaviour : UnityEngine.MonoBehaviour { }
}
#endif

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CameraMovementTests
{
    [UnityTest]
    public IEnumerator CameraDoesNotMoveWithoutInput()
    {
        GameObject go = new GameObject("CameraTest");
        CameraMovement cameraMovement = go.AddComponent<CameraMovement>();
        go.transform.position = Vector3.zero;
        yield return null;
        Assert.AreEqual(Vector3.zero, go.transform.position);
        Assert.AreEqual(Quaternion.identity, go.transform.rotation);
    }
}
