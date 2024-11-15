using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public float moveSpeed = 5.0f;

    void Update()
    {
        // 获取用户输入
        float moveHorizontal = Input.GetAxis("Horizontal"); // A 和 D 控制左右移动
        float moveVertical = Input.GetAxis("Vertical"); // W 和 S 控制前后移动

        // 计算移动方向
        Vector3 movement = new Vector3(moveHorizontal, 0.0f, moveVertical);
        
        // 移动相机
        transform.Translate(movement * moveSpeed * Time.deltaTime, Space.World);
    }
}
