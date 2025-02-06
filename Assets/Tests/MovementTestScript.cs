using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

public class MovementTestScript : InputTestFixture
{
    private Mouse testMouse;

    [SetUp]
    public override void Setup()
    {
        base.Setup();
        testMouse = InputSystem.AddDevice<Mouse>(); // Add a virtual mouse device
    }

    [Test]
    public void test_ObjectMovementWithCamera()
    {
        var gameObject = new GameObject("Player");
        var movementScript = gameObject.AddComponent<ObjectMovementWithCamera>();
        movementScript.moveSpeed = 5.0f;

        var cameraObject = new GameObject("Camera");
        movementScript.cameraTransform = cameraObject.transform;

        // Simulate camera position and orientation
        cameraObject.transform.position = Vector3.zero;
        cameraObject.transform.rotation = Quaternion.identity;

        // Mock keyboard input
        Vector2 input = new Vector2(10, 10); // Forward and Right movement
        var horizontalInput = input.x; // Mock Horizontal
        var verticalInput = input.y;   // Mock Vertical

        // Mock forward and right directions from the camera
        Vector3 expectedForward = cameraObject.transform.forward;
        Vector3 expectedRight = cameraObject.transform.right;
        expectedForward.y = 0; expectedRight.y = 0;
        expectedForward.Normalize(); expectedRight.Normalize();

        // Calculate expected movement
        float deltaTime = 0.1f;
        Time.timeScale = deltaTime; // Mock time step
    }

    [Test]
    public void test_PitchClampWithCamera()
    {
        var gameObject = new GameObject("Player");
        var movementScript = gameObject.AddComponent<ObjectMovementWithCamera>();
        

        var cameraObject = new GameObject("Camera");
        movementScript.cameraTransform = cameraObject.transform;

        // Simulate camera position and orientation
        cameraObject.transform.position = Vector3.zero;
        cameraObject.transform.rotation = Quaternion.identity;

        //movementScript.isMoving = true;
        Press(testMouse.leftButton);
        movementScript.pitch = 100.0f;
        movementScript.Update();
        Assert.AreEqual(movementScript.pitch, 90.0f);
        Vector3 testVector = new Vector3(90, 0, 0);
        //Assert.AreEqual(gameObject.transform.eulerAngles, testVector);

    }

    [Test]
    public void DoesNotMoveOrRotateWithNoInput()
    {
        // Given
        var gameObject = new GameObject("Player");
        var movementScript = gameObject.AddComponent<ObjectMovementWithCamera>();
        var initialPosition = gameObject.transform.position;
        var initialRotation = gameObject.transform.eulerAngles;

        movementScript.Update(); 

        // Assert
        Assert.AreEqual(initialPosition, gameObject.transform.position);
        Assert.AreEqual(initialRotation, gameObject.transform.eulerAngles);
    }

    [Test]
    public void LogsWarningWhenCameraTransformIsNull()
    {
        // Given
        var gameObject = new GameObject("Player");
        var movementScript = gameObject.AddComponent<ObjectMovementWithCamera>();

        movementScript.cameraTransform = null;
        movementScript.Update();

        // Assert
        LogAssert.Expect(LogType.Warning, "without cameraTransform ,please add it in the inspector.");
    }

}
