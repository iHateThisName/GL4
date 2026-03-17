using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MonsterSystem
{
    /// <summary>
    /// Custom inspector for <see cref="MonsterConfig"/> and its subclasses.
    /// Automatically discovers SpawnPoint array fields and draws drag-and-drop helpers for each.
    /// </summary>
    [CustomEditor(typeof(MonsterConfig), true)]
    public class MonsterConfigEditor : Editor
    {
        // Cached lists of transforms keyed by serialized property name, used for the drag-and-drop helper per SpawnPoint array
        private readonly Dictionary<string, List<Transform>> transformLists = new();

        /// <summary>
        /// Draws the default inspector followed by a drag-and-drop spawn-point helper
        /// for every SpawnPoint array found on the target config.
        /// </summary>
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            // Iterate all visible serialized properties to find SpawnPoint arrays
            var iterator = this.serializedObject.GetIterator();
            while (iterator.NextVisible(true))
            {
                // Only process properties that are arrays of SpawnPoint structs
                if (iterator.isArray && iterator.arrayElementType == "SpawnPoint")
                {
                    // Lazily create a transform list for this property if one does not exist yet
                    if (!this.transformLists.ContainsKey(iterator.name))
                        this.transformLists[iterator.name] = new List<Transform>();

                    EditorGUILayout.Space();
                    // Draw the helper UI for this specific SpawnPoint array
                    this.DrawSpawnPointHelper(iterator.name, this.transformLists[iterator.name]);
                }
            }
        }

        /// <summary>
        /// Renders the drag-and-drop area, transform list, and capture button
        /// for a single SpawnPoint array property.
        /// </summary>
        /// <param name="propertyName">The serialized property name of the SpawnPoint array.</param>
        /// <param name="transforms">The working list of scene transforms to capture from.</param>
        private void DrawSpawnPointHelper(string propertyName, List<Transform> transforms)
        {
            // Convert the camelCase property name into a human-readable label
            string displayName = ObjectNames.NicifyVariableName(propertyName);

            EditorGUILayout.LabelField($"{displayName} — Scene Helper", EditorStyles.boldLabel);

            // Create the drag-and-drop target rectangle
            var dropArea = GUILayoutUtility.GetRect(0, 40, GUILayout.ExpandWidth(true));
            GUI.Box(dropArea, "Drag GameObjects / Transforms here", EditorStyles.helpBox);
            SpawnPointEditorHelper.HandleDragAndDrop(dropArea, transforms);

            // Draw the editable list of currently staged transforms
            SpawnPointEditorHelper.DrawTransformList(transforms);

            EditorGUILayout.Space();

            // Disable the capture button when there are no transforms staged
            GUI.enabled = transforms.Count > 0;
            if (GUILayout.Button($"Capture {displayName}"))
            {
                // Write the staged transforms into the serialized SpawnPoint array
                SpawnPointEditorHelper.CaptureSpawnPoints(this.serializedObject, this.target, propertyName, transforms);
                transforms.Clear();
            }
            GUI.enabled = true;
        }
    }
}
