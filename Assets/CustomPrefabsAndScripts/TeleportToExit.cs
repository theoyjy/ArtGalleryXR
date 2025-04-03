using UnityEngine;
using UnityEngine.XR;

public class TeleportToExit : MonoBehaviour
{

    private UnityEngine.XR.InputDevice leftController;
    private float debounceTime = 0.3f;
    private float lastPressedTime = 0f;
    public GameObject teleportPoint;

    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private bool isExit = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isExit = false;
        leftController = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
        teleportPoint = GameObject.FindGameObjectWithTag("Exit");
    }

    // Update is called once per frame
    void Update()
    {
        if (leftController.TryGetFeatureValue(CommonUsages.secondaryButton, out bool yPressed))
        {
            if (yPressed && Time.time - lastPressedTime > debounceTime)
            {
                lastPressedTime = Time.time;
                isExit = !isExit;
                if (isExit)
                {
                    lastPosition = gameObject.transform.position;
                    lastRotation = gameObject.transform.rotation;
                    gameObject.transform.position = teleportPoint.transform.position;
                    gameObject.transform.rotation = teleportPoint.transform.rotation;
                }
                else
                {
                    gameObject.transform.position = lastPosition;
                    gameObject.transform.rotation = lastRotation;
                }

                
            }
        }
    }
}
