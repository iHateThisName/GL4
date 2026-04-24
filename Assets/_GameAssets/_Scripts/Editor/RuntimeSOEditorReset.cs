#if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoad]
public static class RuntimeSOEditorReset
{
    static RuntimeSOEditorReset()
    {
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private static void OnPlayModeStateChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
            SO_RuntimeScriptableObject.ResetAllForEditor();
    }
}
#endif