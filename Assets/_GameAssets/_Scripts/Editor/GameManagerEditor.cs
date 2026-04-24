using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameManager))]
public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();

        var debugSpawnProp = serializedObject.FindProperty("debugSpawn");
        if (!debugSpawnProp.boolValue) return;

        var nightSettingsProp = serializedObject.FindProperty("nightSettings");
        var nightSettings = nightSettingsProp.objectReferenceValue as SO_NightSettings;
        if (nightSettings == null)
        {
            EditorGUILayout.HelpBox("Assign Night Settings to configure debug spawns.", MessageType.Warning);
            return;
        }

        var allMonsters = new List<GameObject>();
        var seen = new HashSet<GameObject>();
        nightSettings.ForEachEventAcrossAllNights(evt =>
        {
            if (evt.GetEventType() != NightEvent.NightEventType.SpawnMonster) return;
            var prefab = evt.GetMonsterPrefab();
            if (prefab != null && seen.Add(prefab))
                allMonsters.Add(prefab);
        });

        if (allMonsters.Count == 0)
        {
            EditorGUILayout.HelpBox("No monster spawn events found in Night Settings.", MessageType.Info);
            return;
        }

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Debug Spawn Selection", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("All selected by default. Uncheck to exclude, or use Deselect All.", MessageType.None);

        var selectionProp = serializedObject.FindProperty("debugSpawnSelection");
        var initializedProp = serializedObject.FindProperty("debugSpawnSelectionInitialized");

        var currentSelection = new HashSet<GameObject>();
        for (int i = 0; i < selectionProp.arraySize; i++)
        {
            var obj = selectionProp.GetArrayElementAtIndex(i).objectReferenceValue as GameObject;
            if (obj != null) currentSelection.Add(obj);
        }

        // Auto-select all monsters the very first time debugSpawn is enabled.
        if (!initializedProp.boolValue)
        {
            currentSelection = new HashSet<GameObject>(allMonsters);
            selectionProp.ClearArray();
            int i = 0;
            foreach (var monster in currentSelection)
            {
                selectionProp.InsertArrayElementAtIndex(i);
                selectionProp.GetArrayElementAtIndex(i).objectReferenceValue = monster;
                i++;
            }
            initializedProp.boolValue = true;
            serializedObject.ApplyModifiedProperties();
        }

        bool changed = false;
        foreach (var monster in allMonsters)
        {
            bool wasSelected = currentSelection.Contains(monster);
            bool isSelected = EditorGUILayout.Toggle(monster.name, wasSelected);
            if (isSelected == wasSelected) continue;
            if (isSelected) currentSelection.Add(monster);
            else currentSelection.Remove(monster);
            changed = true;
        }

        EditorGUILayout.Space();
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Select All"))
        {
            currentSelection = new HashSet<GameObject>(allMonsters);
            changed = true;
        }
        if (GUILayout.Button("Deselect All"))
        {
            currentSelection.Clear();
            changed = true;
        }
        EditorGUILayout.EndHorizontal();

        if (!changed) return;

        selectionProp.ClearArray();
        int idx = 0;
        foreach (var monster in currentSelection)
        {
            selectionProp.InsertArrayElementAtIndex(idx);
            selectionProp.GetArrayElementAtIndex(idx).objectReferenceValue = monster;
            idx++;
        }
        serializedObject.ApplyModifiedProperties();
    }
}
