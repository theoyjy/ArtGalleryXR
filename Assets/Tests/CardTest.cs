using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class CardTests
{
    private GameObject cardObj;
    private Card card;
    private Mesh emptyMesh;

    [SetUp]
    public void Setup()
    {
        cardObj = new GameObject("Card");
        cardObj.AddComponent<MeshFilter>();
        card = cardObj.AddComponent<Card>();

        // Assign a dummy mesh
        emptyMesh = new Mesh();
        typeof(Card).GetField("EmptyMesh", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .SetValue(card, emptyMesh);

        // Manually call Start since Unity doesn't in tests
        card.SendMessage("Start");
    }

    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(cardObj);
    }

    [Test]
    public void Start_InitializesMeshFilterWithEmptyMesh()
    {
        MeshFilter meshFilter = cardObj.GetComponent<MeshFilter>();
        Assert.AreEqual(emptyMesh, meshFilter.mesh);
    }

    [Test]
    public void GetWorldLoc_ReturnsCorrectPosition()
    {
        Vector3 testPos = new Vector3(1, 2, 3);
        cardObj.transform.position = testPos;
        Assert.AreEqual(testPos, card.GetWorldLoc());
    }

    [Test]
    public void GetWorldQuatRot_ReturnsFlippedParentRotation()
    {
        GameObject parent = new GameObject("Parent");
        parent.transform.rotation = Quaternion.Euler(0, 90, 0);
        cardObj.transform.parent = parent.transform;

        Quaternion expected = parent.transform.rotation * Quaternion.Euler(0f, 180f, 0f);
        Assert.AreEqual(expected.eulerAngles, card.GetWorldQuatRot().eulerAngles);
    }

    [Test]
    public void GetNormal_ReturnsExpectedForwardDirection()
    {
        GameObject parent = new GameObject("Parent");
        parent.transform.rotation = Quaternion.Euler(0, 90, 0);
        cardObj.transform.parent = parent.transform;

        Vector3 expected = (parent.transform.rotation * Quaternion.Euler(0f, 180f, 0f)) * Vector3.forward;
        Assert.AreEqual(expected, card.GetNormal());
    }

    [UnityTest]
    public IEnumerator RotateCard_ChangesRotation()
    {
        // Fix: Use MonoBehaviour.StartCoroutine instead of GameObject.StartCoroutine
        yield return card.StartCoroutine("RotateCard");
        Assert.AreEqual(180f, cardObj.transform.localRotation.eulerAngles.y, 1f);
    }
}
