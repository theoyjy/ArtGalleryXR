using UnityEngine;

public class ObjectMovementWithCamera : MonoBehaviour
{
    public float moveSpeed = 5.0f; // 物体移动速度
    public float rotationSpeed = 3.0f; // 视角旋转速度

    public Transform cameraTransform; // 绑定的相机Transform
    private float yaw = 0.0f; // 水平角度
    private float pitch = 0.0f; // 垂直角度

    void Update()
    {
        // 鼠标拖动控制视角
        if (Input.GetMouseButton(0)) // 按住鼠标左键
        {
            yaw += Input.GetAxis("Mouse X") * rotationSpeed; // 水平旋转
            pitch -= Input.GetAxis("Mouse Y") * rotationSpeed; // 垂直旋转

            // 限制垂直旋转范围
            pitch = Mathf.Clamp(pitch, -90f, 90f);

            // 更新相机旋转
            transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        }

        // 键盘输入控制物体移动
        float moveHorizontal = Input.GetAxis("Horizontal"); // A 和 D 键，左右移动
        float moveVertical = Input.GetAxis("Vertical"); // W 和 S 键，前后移动

        // 从相机Transform获取方向
        if (cameraTransform != null)
        {
            // 获取相机方向
            Vector3 forward = cameraTransform.forward; // 相机的前方向
            Vector3 right = cameraTransform.right; // 相机的右方向

            // 忽略垂直方向的影响，只在水平平面移动
            forward.y = 0;
            right.y = 0;
            forward.Normalize();
            right.Normalize();

            // 根据输入计算移动向量
            Vector3 movement = (forward * moveVertical + right * moveHorizontal) * moveSpeed * Time.deltaTime;

            // 移动物体
            transform.Translate(movement, Space.World);
        }
        else
        {
            Debug.LogWarning("without cameraTransform ,please add it in the inspector.");
        }
    }
}
