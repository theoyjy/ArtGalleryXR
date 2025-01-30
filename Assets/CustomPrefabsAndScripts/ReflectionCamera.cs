using UnityEngine;

[RequireComponent(typeof(Camera))]
public class ReflectionCamera : MonoBehaviour
{
    public Transform reflectionPlane; // 平面的位置，用于计算反射
    public RenderTexture renderTexture; // 渲染反射图像的 RenderTexture

    private Camera mainCamera;
    private Camera reflectionCamera;

    private void Start()
    {
        // 获取主相机
        mainCamera = GetComponent<Camera>();

        // 自动创建反射相机
        CreateReflectionCamera();
    }

    private void CreateReflectionCamera()
    {
        // 创建新的相机对象
        GameObject reflectionCamObj = new GameObject("Reflection Camera");
        reflectionCamObj.transform.SetParent(transform); // 设置为主相机的子对象

        // 添加 Camera 组件并设置参数
        reflectionCamera = reflectionCamObj.AddComponent<Camera>();
        reflectionCamera.enabled = false; // 不直接渲染到屏幕

        // 同步主相机的设置
        reflectionCamera.CopyFrom(mainCamera);

        // 设置渲染目标
        if (renderTexture != null)
        {
            reflectionCamera.targetTexture = renderTexture;
        }
        else
        {
            Debug.LogError("RenderTexture is not assigned!");
        }
    }

    private void LateUpdate()
    {
        if (reflectionPlane == null || renderTexture == null)
        {
            Debug.LogWarning("Reflection Plane or RenderTexture is not assigned!");
            return;
        }

        // 获取平面法线和位置
        Vector3 planeNormal = reflectionPlane.up; // 平面法线方向
        Vector3 planePosition = reflectionPlane.position;

        // 计算反射矩阵
        Vector4 plane = new Vector4(planeNormal.x, planeNormal.y, planeNormal.z, -Vector3.Dot(planeNormal, planePosition));
        Matrix4x4 reflectionMatrix = CalculateReflectionMatrix(plane);

        // 设置反射相机的位置和方向
        Vector3 reflectedPosition = reflectionMatrix.MultiplyPoint(mainCamera.transform.position);
        reflectionCamera.transform.position = reflectedPosition;

        Vector3 reflectedForward = reflectionMatrix.MultiplyVector(mainCamera.transform.forward);
        Vector3 reflectedUp = reflectionMatrix.MultiplyVector(mainCamera.transform.up);
        reflectionCamera.transform.rotation = Quaternion.LookRotation(reflectedForward, reflectedUp);

        // 设置反射相机的剪裁平面
        Vector4 clipPlane = CameraSpacePlane(reflectionCamera, planePosition, planeNormal, 1.0f);
        reflectionCamera.projectionMatrix = mainCamera.CalculateObliqueMatrix(clipPlane);

        // 渲染到 RenderTexture
        reflectionCamera.Render();
    }

    private Matrix4x4 CalculateReflectionMatrix(Vector4 plane)
    {
        Matrix4x4 reflectionMatrix = Matrix4x4.zero;
        reflectionMatrix.m00 = 1f - 2f * plane.x * plane.x;
        reflectionMatrix.m01 = -2f * plane.x * plane.y;
        reflectionMatrix.m02 = -2f * plane.x * plane.z;
        reflectionMatrix.m03 = -2f * plane.w * plane.x;

        reflectionMatrix.m10 = -2f * plane.y * plane.x;
        reflectionMatrix.m11 = 1f - 2f * plane.y * plane.y;
        reflectionMatrix.m12 = -2f * plane.y * plane.z;
        reflectionMatrix.m13 = -2f * plane.w * plane.y;

        reflectionMatrix.m20 = -2f * plane.z * plane.x;
        reflectionMatrix.m21 = -2f * plane.z * plane.y;
        reflectionMatrix.m22 = 1f - 2f * plane.z * plane.z;
        reflectionMatrix.m23 = -2f * plane.w * plane.z;

        reflectionMatrix.m33 = 1f;

        return reflectionMatrix;
    }

    private Vector4 CameraSpacePlane(Camera cam, Vector3 position, Vector3 normal, float sideSign)
    {
        Vector3 offsetPos = position + normal * 0.07f; // 避免浮点误差
        Matrix4x4 worldToCameraMatrix = cam.worldToCameraMatrix;
        Vector3 cameraPosition = worldToCameraMatrix.MultiplyPoint(offsetPos);
        Vector3 cameraNormal = worldToCameraMatrix.MultiplyVector(normal).normalized * sideSign;

        return new Vector4(cameraNormal.x, cameraNormal.y, cameraNormal.z, -Vector3.Dot(cameraPosition, cameraNormal));
    }
}
