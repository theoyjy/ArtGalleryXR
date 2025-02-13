using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectMovementWithCamera : MonoBehaviour
{
    public float moveSpeed = 5.0f; // 物体移动速度
    public float rotationSpeed = 1.5f; // 视角旋转速度

    public Transform cameraTransform; // 绑定的相机Transform
    public float yaw = 0.0f; // 水平角度
    public float pitch = 0.0f; // 垂直角度

    private Vector2 lastMousePosition; // 记录上一次鼠标位置
    private bool isDragging = false; // 是否正在拖动

    public void Update()
    {
        InputSystem.Update();

        // 鼠标拖动控制视角
        if (Mouse.current.leftButton.isPressed)
        {
            Vector2 mousePosition = Mouse.current.position.ReadValue();

            if (!isDragging) // 进入拖动模式
            {
                isDragging = true;
                lastMousePosition = mousePosition; // 记录初始位置
            }

            // 计算鼠标移动量
            Vector2 delta = mousePosition - lastMousePosition;
            lastMousePosition = mousePosition; // 更新鼠标位置

            // 旋转计算
            yaw += delta.x * rotationSpeed * 0.1f; // 水平旋转
            pitch -= delta.y * rotationSpeed * 0.1f; // 垂直旋转
            pitch = Mathf.Clamp(pitch, -90f, 90f);

            // 更新相机旋转
            transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        }
        else
        {
            isDragging = false; // 释放鼠标后，重置拖动状态
        }

        // 键盘输入控制物体移动
        float moveHorizontal = Input.GetAxis("Horizontal");
        float moveVertical = Input.GetAxis("Vertical");

        if (cameraTransform != null)
        {
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;

            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            Vector3 movement = (forward * moveVertical + right * moveHorizontal) * moveSpeed * Time.deltaTime;
            transform.Translate(movement, Space.World);
        }
        else
        {
            Debug.LogWarning("without cameraTransform ,please add it in the inspector.");
        }
    }
}
