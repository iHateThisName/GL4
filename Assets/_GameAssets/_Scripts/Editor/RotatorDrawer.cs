using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RotatorInteractable))]
public class RotatorDrawer : Editor
{
    private const float DELTA = 30f;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RotatorInteractable rotator = (RotatorInteractable)target;

        GUILayout.Space(10);
        GUILayout.Label("Editor Test Controls", EditorStyles.boldLabel);

        using (new GUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Rotate -30°"))
            {
                Undo.RecordObject(rotator, "Rotate Crank -30");
                rotator.EditorRotate(-DELTA);
                EditorUtility.SetDirty(rotator);
            }

            if (GUILayout.Button("Rotate +30°"))
            {
                Undo.RecordObject(rotator, "Rotate Crank +30");
                rotator.EditorRotate(DELTA);
                EditorUtility.SetDirty(rotator);
            }
        }
    }
}
