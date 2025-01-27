using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class MovementTestScript
{
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
