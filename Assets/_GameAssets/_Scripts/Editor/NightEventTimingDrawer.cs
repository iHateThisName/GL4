using UnityEditor;
using UnityEngine;

/// <summary>
/// Hides the unused single/range fields on <see cref="NightEventTiming"/> based on
/// the <c>useRange</c> toggle, and shows the resolved seconds-against-night preview.
/// </summary>
[CustomPropertyDrawer(typeof(NightEventTiming))]
public class NightEventTimingDrawer : PropertyDrawer
{
    // Cached night length, populated eagerly to avoid any AssetDatabase calls inside OnGUI.
    private static float cachedNightSeconds = -1f;

    [InitializeOnLoadMethod]
    private static void Hook()
    {
        // Populate the cache immediately on domain load.
        RefreshCache();
        // Re-populate asynchronously after project changes — delayCall breaks the
        // sync feedback loop that LoadAssetAtPath would otherwise cause in OnGUI.
        EditorApplication.projectChanged += () => EditorApplication.delayCall += RefreshCache;
    }

    private static void RefreshCache()
    {
        var guids = AssetDatabase.FindAssets("t:SO_NightSettings");
        if (guids == null || guids.Length == 0)
        {
            cachedNightSeconds = 8f * 60f;
            return;
        }
        var path = AssetDatabase.GUIDToAssetPath(guids[0]);
        var settings = AssetDatabase.LoadAssetAtPath<SO_NightSettings>(path);
        cachedNightSeconds = settings != null ? settings.GetNightTimeInSeconds() : 8f * 60f;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var useRange = property.FindPropertyRelative("useRange");
        var time = property.FindPropertyRelative("time");
        var timeMin = property.FindPropertyRelative("timeMin");
        var timeMax = property.FindPropertyRelative("timeMax");

        float lineHeight = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;

        Rect headerRect = new Rect(position.x, position.y, position.width, lineHeight);
        property.isExpanded = EditorGUI.Foldout(headerRect, property.isExpanded, label, true);

        if (!property.isExpanded)
        {
            EditorGUI.EndProperty();
            return;
        }

        Rect line = new Rect(position.x, position.y + lineHeight + spacing, position.width, lineHeight);

        EditorGUI.indentLevel++;

        EditorGUI.PropertyField(line, useRange);
        line.y += lineHeight + spacing;

        if (useRange.boolValue)
        {
            EditorGUI.PropertyField(line, timeMin);
            line.y += lineHeight + spacing;
            EditorGUI.PropertyField(line, timeMax);
            line.y += lineHeight + spacing;
        }
        else
        {
            EditorGUI.PropertyField(line, time);
            line.y += lineHeight + spacing;
        }

        // Seconds preview against the resolved night length.
        float nightSeconds = ResolveNightSeconds();
        string preview;
        if (useRange.boolValue)
        {
            float minS = Mathf.Min(timeMin.floatValue, timeMax.floatValue) * nightSeconds;
            float maxS = Mathf.Max(timeMin.floatValue, timeMax.floatValue) * nightSeconds;
            preview = $"≈ {FormatTime(minS)} – {FormatTime(maxS)} of {FormatTime(nightSeconds)}";
        }
        else
        {
            float s = Mathf.Clamp01(time.floatValue) * nightSeconds;
            preview = $"≈ {FormatTime(s)} of {FormatTime(nightSeconds)}";
        }
        EditorGUI.LabelField(line, " ", preview, EditorStyles.miniLabel);

        EditorGUI.indentLevel--;
        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        float line = EditorGUIUtility.singleLineHeight;
        float spacing = EditorGUIUtility.standardVerticalSpacing;
        if (!property.isExpanded) return line;

        var useRange = property.FindPropertyRelative("useRange");
        // header + useRange + (1 single OR 2 range) + preview line
        int rows = 3 + (useRange.boolValue ? 2 : 1);
        return rows * line + (rows - 1) * spacing;
    }

    private static string FormatTime(float seconds)
    {
        if (seconds < 60f) return $"{seconds:F1}s";
        int m = Mathf.FloorToInt(seconds / 60f);
        float s = seconds - m * 60f;
        return $"{m}m {s:F0}s";
    }

    /// <summary>
    /// Returns the cached night length in seconds. The cache is populated by
    /// <see cref="RefreshCache"/> at domain load and after project changes, so
    /// this method never touches the AssetDatabase (safe to call from OnGUI).
    /// </summary>
    private static float ResolveNightSeconds()
    {
        return cachedNightSeconds > 0f ? cachedNightSeconds : 8f * 60f;
    }
}
