using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class SettingsScript : MonoBehaviour
{
    //Refrence to the settings bools
    [SerializeField] private SettingsBools settingsBool;

    //These cubes are just for visuals, they don't do anything, they turn off and on again depending on what the player wants
    [SerializeField] private GameObject enableSnapCube;
    [SerializeField] private GameObject enableSmoothCube;

    [SerializeField] private GameObject tabledSnapCube;
    [SerializeField] private GameObject tabledSmoothCube;

    [SerializeField] private GameObject enableTunnelingCube;
    [SerializeField] private GameObject disableTunnelingCube;

    [SerializeField] private GameObject tabledTunnelingCube;
    [SerializeField] private GameObject tabledNoTunnelCube;

    [SerializeField] private GameObject enableTelportCube;
    [SerializeField] private GameObject enabledLocomotionCube;

    [SerializeField] private GameObject tabledTeleportCube;
    [SerializeField] private GameObject tabledLocomotionCube;

    [SerializeField] private GameObject enableNightOneCube;
    [SerializeField] private GameObject enableNightTwoCube;
    [SerializeField] private GameObject enableNightThreeCube;
    [SerializeField] private GameObject enableNightFourCube;
    [SerializeField] private GameObject enableNightFiveCube;

    [SerializeField] private GameObject tabledNightOneCube;
    [SerializeField] private GameObject tabledNightTwoCube;
    [SerializeField] private GameObject tabledNightThreeCube;
    [SerializeField] private GameObject tabledNightFourCube;
    [SerializeField] private GameObject tabledNightFiveCube;

    //A refrence to the player's controllers
    [SerializeField] private ControllerInputActionManager rightControllerRefrence;
    [SerializeField] private ControllerInputActionManager leftControllerRefrence;

    //A refrence to the tunneling vignette that's used for the tunneling
    [SerializeField] private GameObject tunnelingVignette;

    //A refrence to the night settings
    [SerializeField] private SO_NightSettings nightSettings;

    [Header("Teleport Input Actions")]
    [Tooltip("Drag the Right Teleport Activate action here")]
    public InputActionReference teleportActivateAction;

    [Tooltip("Drag the Right Teleport Cancel action here")]
    public InputActionReference teleportCancelAction;

    private void Start()
    {
        //Set default settings
        tunnelingVignette.SetActive(false);
        rightControllerRefrence.smoothTurnEnabled = true;
        leftControllerRefrence.smoothMotionEnabled = true;

        settingsBool = GameObject.Find("SettingsBools").GetComponent<SettingsBools>();

        //Setting the default night to the current night
        nightSettings.SetDebugStartNight(this.settingsBool.currentNight);

        if(settingsBool.snapEnabled)
        {
            EnableSnapTurn();
        }
        if(settingsBool.tunnelingEnabled)
        {
            EnableTunneling();
        }
        if(settingsBool.teleportEnabled)
        {
            EnableTeleport();
        }
    }

    //A method for enabling snap turning
    public void EnableSnapTurn()
    {
        rightControllerRefrence.smoothTurnEnabled = false;
        leftControllerRefrence.smoothTurnEnabled = false;
        settingsBool.snapEnabled = true;
        enableSnapCube.SetActive(false);
        tabledSnapCube.SetActive(true);
        enableSmoothCube.SetActive(true);
        tabledSmoothCube.SetActive(false);
    }

    //A method for disabling snap turning
    public void DisableSnapTurn()
    {
        rightControllerRefrence.smoothTurnEnabled = true;
        leftControllerRefrence.smoothTurnEnabled = true;
        settingsBool.snapEnabled = false;
        enableSmoothCube.SetActive(false);
        tabledSmoothCube.SetActive(true);
        enableSnapCube.SetActive(true);
        tabledSnapCube.SetActive(false);
    }

    //A method for enabling tunneling
    public void EnableTunneling()
    {
        tunnelingVignette.SetActive(true);
        settingsBool.tunnelingEnabled = true;
        enableTunnelingCube.SetActive(false);
        tabledTunnelingCube.SetActive(true);
        disableTunnelingCube.SetActive(true);
        tabledNoTunnelCube.SetActive(false);
    }

    //A method for disabling tunneling
    public void DisableTunneling()
    {
        tunnelingVignette.SetActive(false);
        settingsBool.tunnelingEnabled = false;
        disableTunnelingCube.SetActive(false);
        tabledNoTunnelCube.SetActive(true);
        enableTunnelingCube.SetActive(true);
        tabledTunnelingCube.SetActive(false);
    }

    //A method for enabling teleporting
    public void EnableTeleport()
    {
        leftControllerRefrence.smoothMotionEnabled = false;
        settingsBool.teleportEnabled = true;
        SetTeleportEnabled(true);
        enableTelportCube.SetActive(false);
        tabledTeleportCube.SetActive(true);
        enabledLocomotionCube.SetActive(true);
        tabledLocomotionCube.SetActive(false);
    }

    //A method for disabling teleporting
    public void DisableTeleport()
    {
        leftControllerRefrence.smoothMotionEnabled = true;
        settingsBool.teleportEnabled = false;
        SetTeleportEnabled(false);
        enabledLocomotionCube.SetActive(false);
        tabledLocomotionCube.SetActive(true);
        enableTelportCube.SetActive(true);
        tabledTeleportCube.SetActive(false);
    }

    public void SetCurrentNight(int nightNum)
    {
        if (nightNum == 1)
        {
            nightSettings.SetDebugStartNight(nightNum);

            enableNightOneCube.SetActive(false);
            enableNightTwoCube.SetActive(true);
            enableNightThreeCube.SetActive(true);
            enableNightFourCube.SetActive(true);
            enableNightFiveCube.SetActive(true);

            tabledNightOneCube.SetActive(true);
            tabledNightTwoCube.SetActive(false);
            tabledNightThreeCube.SetActive(false);
            tabledNightFourCube.SetActive(false);
            tabledNightFiveCube.SetActive(false);
        }
        else if (nightNum == 2)
        {
            nightSettings.SetDebugStartNight(nightNum);

            enableNightOneCube.SetActive(true);
            enableNightTwoCube.SetActive(false);
            enableNightThreeCube.SetActive(true);
            enableNightFourCube.SetActive(true);
            enableNightFiveCube.SetActive(true);

            tabledNightOneCube.SetActive(false);
            tabledNightTwoCube.SetActive(true);
            tabledNightThreeCube.SetActive(false);
            tabledNightFourCube.SetActive(false);
            tabledNightFiveCube.SetActive(false);

            Debug.Log("Night 2 selected");
        }
        else if (nightNum == 3)
        {
            nightSettings.SetDebugStartNight(nightNum);

            enableNightOneCube.SetActive(true);
            enableNightTwoCube.SetActive(true);
            enableNightThreeCube.SetActive(false);
            enableNightFourCube.SetActive(true);
            enableNightFiveCube.SetActive(true);

            tabledNightOneCube.SetActive(false);
            tabledNightTwoCube.SetActive(false);
            tabledNightThreeCube.SetActive(true);
            tabledNightFourCube.SetActive(false);
            tabledNightFiveCube.SetActive(false);
        }
        else if (nightNum == 4)
        {
            nightSettings.SetDebugStartNight(nightNum);

            enableNightOneCube.SetActive(true);
            enableNightTwoCube.SetActive(true);
            enableNightThreeCube.SetActive(true);
            enableNightFourCube.SetActive(false);
            enableNightFiveCube.SetActive(true);

            tabledNightOneCube.SetActive(false);
            tabledNightTwoCube.SetActive(false);
            tabledNightThreeCube.SetActive(false);
            tabledNightFourCube.SetActive(true);
            tabledNightFiveCube.SetActive(false);
        }
        else if (nightNum == 5)
        {
            nightSettings.SetDebugStartNight(nightNum);

            enableNightOneCube.SetActive(true);
            enableNightTwoCube.SetActive(true);
            enableNightThreeCube.SetActive(true);
            enableNightFourCube.SetActive(true);
            enableNightFiveCube.SetActive(false);

            tabledNightOneCube.SetActive(false);
            tabledNightTwoCube.SetActive(false);
            tabledNightThreeCube.SetActive(false);
            tabledNightFourCube.SetActive(false);
            tabledNightFiveCube.SetActive(true);
        }
        else
        {
            Debug.LogError("Invalid night number");
        }

        settingsBool.currentNight = nightNum;
    }

    // This is the function your Main Menu Toggle will call
    public void SetTeleportEnabled(bool isEnabled)
    {
        if (teleportActivateAction != null && teleportActivateAction.action != null)
        {
            if (isEnabled)
            {
                teleportActivateAction.action.Enable();
            }
            else
            {
                teleportActivateAction.action.Disable();
            }
        }

        if (teleportCancelAction != null && teleportCancelAction.action != null)
        {
            if (isEnabled)
            {
                teleportCancelAction.action.Enable();
            }
            else
            {
                teleportCancelAction.action.Disable();
            }
        }
        Debug.Log("Teleportation enabled state set to: " + isEnabled);
    }
}
