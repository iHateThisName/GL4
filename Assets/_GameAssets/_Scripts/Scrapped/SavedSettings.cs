using System;
using System.Collections.Generic;

public enum SettingId
{
    UseSnapTurn
}

[Serializable]
public class XRSettings
{
    private Dictionary<SettingId, bool> bools = new();
    private Dictionary<SettingId, float> floats = new();

    private static readonly Dictionary<SettingId, bool> DefaultBools = new()
    {
        { SettingId.UseSnapTurn, true }
    };

    private static readonly Dictionary<SettingId, float> DefaultFloats = new()
    {
    };

    public static XRSettings Default
    {
        get
        {
            var settings = new XRSettings();
            foreach (var kvp in DefaultBools)
                settings.bools[kvp.Key] = kvp.Value;
            foreach (var kvp in DefaultFloats)
                settings.floats[kvp.Key] = kvp.Value;
            return settings;
        }
    }

    public bool GetBool(SettingId id)
    {
        if (bools.TryGetValue(id, out var value))
            return value;
        if (DefaultBools.TryGetValue(id, out var defaultValue))
            return defaultValue;
        return false;
    }

    public void SetBool(SettingId id, bool value)
    {
        bools[id] = value;
    }

    public float GetFloat(SettingId id)
    {
        if (floats.TryGetValue(id, out var value))
            return value;
        if (DefaultFloats.TryGetValue(id, out var defaultValue))
            return defaultValue;
        return 0f;
    }

    public void SetFloat(SettingId id, float value)
    {
        floats[id] = value;
    }
}
