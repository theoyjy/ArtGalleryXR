using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float moveSpeed = 5.0f;  // 相机移动速度
    public float rotationSpeed = 3.0f;  // 相机旋转速度

    private float yaw = 0.0f;  // 水平角度
    private float pitch = 0.0f;  // 垂直角度

    void Update()
    {
        // 鼠标拖动改变视角
        if (Input.GetMouseButton(0)) // 按住左键
        {
            yaw += Input.GetAxis("Mouse X") * rotationSpeed;  // 水平旋转
            pitch -= Input.GetAxis("Mouse Y") * rotationSpeed;  // 垂直旋转

            // 限制垂直旋转范围
            pitch = Mathf.Clamp(pitch, -90f, 90f);

            // 设置相机旋转
            transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        }

        // 键盘输入控制相机移动（基于当前视角方向）
        float moveHorizontal = Input.GetAxis("Horizontal"); // A 和 D 键，左右移动
        float moveVertical = Input.GetAxis("Vertical"); // W 和 S 键，前后移动

        // 计算基于相机方向的移动向量
        Vector3 forward = transform.forward; // 获取当前相机的前方向
        Vector3 right = transform.right; // 获取当前相机的右方向

        // 忽略垂直方向的影响，只保留水平平面的移动
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        // 根据输入和速度计算移动
        Vector3 movement = (forward * moveVertical + right * moveHorizontal) * moveSpeed * Time.deltaTime;

        // 移动相机
        transform.Translate(movement, Space.World);
    }
}
