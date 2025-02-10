#if UNITY_EDITOR
using UnityEditor;
using System;
using System.Reflection;
#endif

public static class GameViewUtils
{
#if UNITY_EDITOR
    public static void SetGameViewSize(int width, int height)
    {
        Type gameViewType = Type.GetType("UnityEditor.GameView,UnityEditor");
        EditorWindow gameView = EditorWindow.GetWindow(gameViewType);
        var method = gameViewType.GetMethod("SizeSelectionCallback",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        
        if (method != null)
        {
            method.Invoke(gameView, new object[] { width, height });
        }

                // **强制更新 Game View**
        EditorApplication.delayCall += () => EditorUtility.SetDirty(gameView);
    }
#endif
}
