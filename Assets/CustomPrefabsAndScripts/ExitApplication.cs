using UnityEngine;

public class ExitApplication : MonoBehaviour
{
    public void Exit()
    {


        Debug.Log("Exitting");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}