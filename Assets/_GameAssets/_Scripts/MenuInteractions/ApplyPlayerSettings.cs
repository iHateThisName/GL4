using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class ApplyPlayerSettings : MonoBehaviour
{
    //A refrence to the settings script
    [SerializeField] private SettingsScript settingsToApply;

    //A refrence to the player's controllers
    [SerializeField] private ControllerInputActionManager rightControllerRefrence;
    [SerializeField] private ControllerInputActionManager leftControllerRefrence;

    //A refrence to the tunneling vignette
    [SerializeField]
    private GameObject tunnelingObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Finds the settings script
        settingsToApply = FindFirstObjectByType<SettingsScript>();

        //Finds the tunneling vignette
        tunnelingObject = GameObject.Find("TunnelingVignette");
        //Applies default settings
        tunnelingObject.SetActive(false);
        rightControllerRefrence.smoothTurnEnabled = true;
        leftControllerRefrence.smoothMotionEnabled = true;

        //Applies the player's preferred settings
        if (settingsToApply != null )
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
