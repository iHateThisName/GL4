using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MonsterSystem
{
    /// <summary>
    /// Custom inspector for <see cref="SO_TransformCollection"/>.
    /// Provides a drag-and-drop helper to populate the navigation points array from scene transforms.
    /// </summary>
    [CustomEditor(typeof(SO_TransformCollection))]
    public class TransformCollectionReferenceEditor : Editor
    {
        // Working list of scene transforms staged for capture into the points array
        private List<Transform> transforms = new();

        /// <summary>
        /// Draws the default inspector followed by a drag-and-drop area
        /// for capturing navigation points from scene transforms.
        /// </summary>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Points — Scene Helper", EditorStyles.boldLabel);

            // Create the drag-and-drop target rectangle
            var dropArea = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Drag GameObjects / Transforms here", EditorStyles.helpBox);
            SpawnPointEditorHelper.HandleDragAndDrop(dropArea, this.transforms);

            // Draw the editable list of currently staged transforms
            SpawnPointEditorHelper.DrawTransformList(this.transforms);

            EditorGUILayout.Space();

            // Disable the capture button when there are no transforms staged
            GUI.enabled = this.transforms.Count > 0;
            if (GUILayout.Button("Capture Points"))
            {
                // Write the staged transforms into the serialized points array
                SpawnPointEditorHelper.CaptureSpawnPoints(this.serializedObject, this.target, "points", this.transforms);
                this.transforms.Clear();
            }
            GUI.enabled = true;
        }
    }
}
