using System;
using System.IO;
using Assets.Scripts.Singleton;
using UnityEngine;
using UnityEngine.XR.Content.Interaction;

public class XRSettingsManager : Singleton<XRSettingsManager>
{
    [SerializeField] private LocomotionManager locomotionManager;

    private XRSettings savedSettings = XRSettings.Default;

    private string SavePath => Path.Combine(Application.persistentDataPath, "settings.json");

    protected override void Awake()
    {
        base.Awake();
        // LoadSettingsFromDisk();
    }

    public bool GetBool(SettingId id) => savedSettings.GetBool(id);

    public float GetFloat(SettingId id) => savedSettings.GetFloat(id);

    public void SaveSettings(XRSettings newSettings)
    {
        savedSettings = newSettings;
        // SaveSettingsToDisk();
        ApplySettings();
    }

    public void ApplySettings()
    {
        if (locomotionManager == null) return;

        var turnStyle = savedSettings.GetBool(SettingId.UseSnapTurn)
            ? LocomotionManager.TurnStyle.Snap
            : LocomotionManager.TurnStyle.Smooth;

        locomotionManager.leftHandTurnStyle = turnStyle;
        locomotionManager.rightHandTurnStyle = turnStyle;
    }

    private void LoadSettingsFromDisk()
    {
        if (File.Exists(SavePath))
        {
            try
            {
                var json = File.ReadAllText(SavePath);
                savedSettings = JsonUtility.FromJson<XRSettings>(json);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Failed to load settings: {e.Message}");
                savedSettings = XRSettings.Default;
            }
        }

        ApplySettings();
    }

    private void SaveSettingsToDisk()
    {
        try
        {
            var json = JsonUtility.ToJson(savedSettings, true);
            File.WriteAllText(SavePath, json);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Failed to save settings: {e.Message}");
        }
    }
}
