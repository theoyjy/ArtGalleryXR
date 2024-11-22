using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class VRDeviceChecker : MonoBehaviour
{
    public GameObject controllerPrefab; // Assign your controller prefab in the Inspector

    void Start()
    {
        CheckVRDevice();
    }

    void CheckVRDevice()
    {
        bool isVRDeviceActive = false;

        // Get all XRInputSubsystems and check if any are running
        var inputSubsystems = new List<XRInputSubsystem>();
        SubsystemManager.GetSubsystems(inputSubsystems);

        foreach (var subsystem in inputSubsystems)
        {
            if (subsystem.running)
            {
                isVRDeviceActive = true;
                break;
            }
        }

        Debug.Log("VR Device Active: " + isVRDeviceActive);
        // Toggle the controller prefab based on VR device status
        if (controllerPrefab != null)
        {
            controllerPrefab.SetActive(isVRDeviceActive);
        }
    }
}
