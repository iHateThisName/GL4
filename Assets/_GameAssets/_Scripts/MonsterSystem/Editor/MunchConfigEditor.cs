using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MonsterSystem
{
    [CustomEditor(typeof(MunchConfig))]
    public class MunchConfigEditor : Editor
    {
        private List<Transform> scenePoints = new List<Transform>();

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Shadow Point Helper", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Drag scene objects below, then click \"Capture All\" to overwrite Shadow Positions with their world positions.",
                MessageType.Info);

            // Draw the transform list
            for (int i = 0; i < scenePoints.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                scenePoints[i] = (Transform)EditorGUILayout.ObjectField(
                    $"Point {i}", scenePoints[i], typeof(Transform), true);

                if (GUILayout.Button("-", GUILayout.Width(24)))
                {
                    scenePoints.RemoveAt(i);
                    i--;
                }

                EditorGUILayout.EndHorizontal();
            }

            if (GUILayout.Button("+ Add Slot"))
                scenePoints.Add(null);

            EditorGUILayout.Space();

            GUI.enabled = scenePoints.Count > 0;
            if (GUILayout.Button("Capture All Positions"))
            {
                var config = (MunchConfig)target;
                Undo.RecordObject(config, "Capture Shadow Positions");

                var positions = new List<Vector3>();
                for (int i = 0; i < scenePoints.Count; i++)
                {
                    if (scenePoints[i] != null)
                        positions.Add(scenePoints[i].position);
                }

                config.shadowPositions = positions.ToArray();
                EditorUtility.SetDirty(config);
            }
            GUI.enabled = true;
        }
    }
}
