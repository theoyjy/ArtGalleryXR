using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.SpatialKeyboard;

public class XRKeyboardNeededCheck : MonoBehaviour
{
    void Awake()
    {
#if !UNITY_ANDROID
        XRKeyboardDisplay xrKeyboard = GetComponent<XRKeyboardDisplay>();
        if (xrKeyboard != null)
        {
            xrKeyboard.enabled = false;
        }
        else
        {
            Debug.LogError("COULD NOT FIND XRKEYBOARDDISPLAY COMPONENT");
        }
#endif
    }
}