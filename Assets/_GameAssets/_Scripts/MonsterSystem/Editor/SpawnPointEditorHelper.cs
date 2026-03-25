using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MonsterSystem
{
    /// <summary>
    /// Static utility class that provides shared drag-and-drop, list drawing,
    /// and capture functionality for spawn-point editor helpers.
    /// </summary>
    public static class SpawnPointEditorHelper
    {
        /// <summary>
        /// Handles Unity editor drag-and-drop events over the given rectangle,
        /// extracting transforms from dragged GameObjects or Components and adding
        /// them to the provided list.
        /// </summary>
        /// <param name="dropArea">The screen rectangle that accepts drag-and-drop.</param>
        /// <param name="transforms">The list to populate with dragged transforms.</param>
        public static void HandleDragAndDrop(Rect dropArea, List<Transform> transforms)
        {
            var evt = Event.current;

            // Ignore events outside the drop area
            if (!dropArea.Contains(evt.mousePosition)) return;

            switch (evt.type)
            {
                // Show a copy cursor while dragging over the drop area
                case EventType.DragUpdated:
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                    evt.Use();
                    break;

                // Accept the drag and extract transforms from the dropped objects
                case EventType.DragPerform:
                    DragAndDrop.AcceptDrag();

                    foreach (var obj in DragAndDrop.objectReferences)
                    {
                        Transform t = null;

                        // Resolve the transform from either a GameObject or a Component
                        if (obj is GameObject go)
                            t = go.transform;
                        else if (obj is Component comp)
                            t = comp.transform;

                        // Add only non-null, unique transforms
                        if (t != null && !transforms.Contains(t))
                            transforms.Add(t);
                    }

                    evt.Use();
                    break;
            }
        }

        /// <summary>
        /// Draws an editable list of transforms with per-entry object fields and remove buttons.
        /// </summary>
        /// <param name="transforms">The list of transforms to display and allow editing of.</param>
        public static void DrawTransformList(List<Transform> transforms)
        {
            for (int i = 0; i < transforms.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();

                // Editable object field for each staged transform
                transforms[i] = (Transform)EditorGUILayout.ObjectField(
                    $"Point {i}", transforms[i], typeof(Transform), true);

                // Remove button to discard this entry from the list
                if (GUILayout.Button("-", GUILayout.Width(24)))
                {
                    transforms.RemoveAt(i);
                    i--; // Adjust index after removal so we don't skip entries
                }

                EditorGUILayout.EndHorizontal();
            }
        }

        /// <summary>
        /// Captures position and rotation from a list of scene transforms and writes
        /// them into the serialized SpawnPoint array property on the target object.
        /// </summary>
        /// <param name="serializedObject">The serialized representation of the target asset.</param>
        /// <param name="target">The Unity Object being edited, used for undo registration.</param>
        /// <param name="propertyName">The name of the serialized SpawnPoint array property.</param>
        /// <param name="transforms">The source transforms whose positions and rotations will be captured.</param>
        public static void CaptureSpawnPoints(SerializedObject serializedObject, Object target, string propertyName, List<Transform> transforms)
        {
            // Record the object state for undo support
            Undo.RecordObject(target, $"Capture {propertyName}");

            // Filter out any null entries before writing
            var validTransforms = new List<Transform>();
            for (int i = 0; i < transforms.Count; i++)
            {
                if (transforms[i] != null)
                    validTransforms.Add(transforms[i]);
            }

            // Locate the serialized property and write position/rotation data
            var prop = serializedObject.FindProperty(propertyName);
            if (prop != null)
            {
                // Resize the array to match the number of valid transforms
                prop.arraySize = validTransforms.Count;
                for (int i = 0; i < validTransforms.Count; i++)
                {
                    var element = prop.GetArrayElementAtIndex(i);
                    // Copy world-space position and euler angles from each transform
                    element.FindPropertyRelative("position").vector3Value = validTransforms[i].position;
                    element.FindPropertyRelative("rotation").vector3Value = validTransforms[i].eulerAngles;
                }

                // Apply all pending property modifications to the serialized object
                serializedObject.ApplyModifiedProperties();
            }

            // Mark the asset as dirty so changes are saved
            EditorUtility.SetDirty(target);
        }
    }
}
