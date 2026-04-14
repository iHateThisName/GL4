using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class ApplyPlayerSettings : MonoBehaviour
{
    [SerializeField]
    private SettingsScript settingsToApply;

    [SerializeField]
    private ControllerInputActionManager rightControllerRefrence;

    [SerializeField]
    private ControllerInputActionManager leftControllerRefrence;

    [SerializeField]
    private GameObject tunnelingObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        settingsToApply = FindFirstObjectByType<SettingsScript>();

        tunnelingObject = GameObject.Find("TunnelingVignette");
        tunnelingObject.SetActive(false);

        if(settingsToApply != null )
        {
            if (settingsToApply.snapEnabled)
            {
                rightControllerRefrence.smoothTurnEnabled = false;
            }
            if (settingsToApply.teleportEnabled)
            {
                leftControllerRefrence.smoothMotionEnabled = false;
            }
            if (settingsToApply.tunnelingEnabled)
            {
                tunnelingObject.SetActive(true);
            }
            else
            {
                tunnelingObject.SetActive(false);
            }
        }
    }
}
