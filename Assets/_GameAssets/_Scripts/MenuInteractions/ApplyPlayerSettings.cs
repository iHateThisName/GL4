using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class ApplyPlayerSettings : MonoBehaviour
{
    [SerializeField]
    private SettingsScript settingsToApply;

    [SerializeField]
    private ControllerInputActionManager controllerRefrence;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        settingsToApply = FindFirstObjectByType<SettingsScript>();

        if(settingsToApply.snapEnabled)
        {
            controllerRefrence.smoothTurnEnabled = false;
        }
    }
}
