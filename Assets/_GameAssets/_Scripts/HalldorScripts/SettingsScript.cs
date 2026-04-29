using Assets.Scripts.Singleton;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class SettingsScript : PersistenSingleton<SettingsScript>
{
    //Bools for enabling certain settings. This is used by the ApplyPlayerSettings script
    public bool snapEnabled = false;
    public bool tunnelingEnabled = false;
    public bool teleportEnabled = false;

    [SerializeField] private int currentNight = 1;

    //These cubes are just for visuals, they don't do anything, they turn off and on again depending on what the player wants
    [SerializeField] private GameObject enableSnapCube;
    [SerializeField] private GameObject disableSnapCube;

    [SerializeField] private GameObject enableTunnelingCube;
    [SerializeField] private GameObject disableTunnelingCube;

    [SerializeField] private GameObject enableTelportCube;
    [SerializeField] private GameObject disableTeleportCube;

    [SerializeField] private GameObject enableNightOneCube;
    [SerializeField] private GameObject enableNightTwoCube;
    [SerializeField] private GameObject enableNightThreeCube;
    [SerializeField] private GameObject enableNightFourCube;
    [SerializeField] private GameObject enableNightFiveCube;

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

        //Setting the default night to the current night
        nightSettings.SetDebugStartNight(this.currentNight);
    }

    //A method for enabling snap turning
    public void EnableSnapTurn()
    {
        rightControllerRefrence.smoothTurnEnabled = false;
        leftControllerRefrence.smoothTurnEnabled = false;
        snapEnabled = true;
        enableSnapCube.SetActive(false);
        disableSnapCube.SetActive(true);
    }

    //A method for disabling snap turning
    public void DisableSnapTurn()
    {
        rightControllerRefrence.smoothTurnEnabled = true;
        leftControllerRefrence.smoothTurnEnabled = true;
        snapEnabled = false;
        disableSnapCube.SetActive(false);
        enableSnapCube.SetActive(true);
    }

    //A method for enabling tunneling
    public void EnableTunneling()
    {
        tunnelingVignette.SetActive(true);
        tunnelingEnabled = true;
        enableTunnelingCube.SetActive(false);
        disableTunnelingCube.SetActive(true);
    }

    //A method for disabling tunneling
    public void DisableTunneling()
    {
        tunnelingVignette.SetActive(false);
        tunnelingEnabled = false;
        disableTunnelingCube.SetActive(false);
        enableTunnelingCube.SetActive(true);
    }

    //A method for enabling teleporting
    public void EnableTeleport()
    {
        leftControllerRefrence.smoothMotionEnabled = false;
        teleportEnabled = true;
        SetTeleportEnabled(true);
        enableTelportCube.SetActive(false);
        disableTeleportCube.SetActive(true);
    }

    //A method for disabling teleporting
    public void DisableTeleport()
    {
        leftControllerRefrence.smoothMotionEnabled = true;
        teleportEnabled = false;
        SetTeleportEnabled(false);
        disableTeleportCube.SetActive(false);
        enableTelportCube.SetActive(true);
    }

    public void SetCurrentNight(int nightNum)
    {
        if (nightNum == 1)
        {
            nightSettings.SetDebugStartNight(nightNum);
            enableNightOneCube.SetActive(false);
            enableNightThreeCube.SetActive(true);
            enableNightTwoCube.SetActive(true);
            enableNightFourCube.SetActive(true);
            enableNightFiveCube.SetActive(true);
        }
        else if (nightNum == 2)
        {
            nightSettings.SetDebugStartNight(nightNum);
            enableNightTwoCube.SetActive(false);
            enableNightThreeCube.SetActive(true);
            enableNightOneCube.SetActive(true);
            enableNightFourCube.SetActive(true);
            enableNightFiveCube.SetActive(true);
        }
        else if (nightNum == 3)
        {
            nightSettings.SetDebugStartNight(nightNum);
            enableNightThreeCube.SetActive(false);
            enableNightTwoCube.SetActive(true);
            enableNightOneCube.SetActive(true);
            enableNightFourCube.SetActive(true);
            enableNightFiveCube.SetActive(true);
        }
        else if (nightNum == 4)
        {
            nightSettings.SetDebugStartNight(nightNum);
            enableNightFourCube.SetActive(false);
            enableNightThreeCube.SetActive(true);
            enableNightTwoCube.SetActive(true);
            enableNightOneCube.SetActive(true);
            enableNightFiveCube.SetActive(true);
        }
        else if (nightNum == 5)
        {
            nightSettings.SetDebugStartNight(nightNum);
            enableNightFiveCube.SetActive(false);
            enableNightFourCube.SetActive(true);
            enableNightThreeCube.SetActive(true);
            enableNightTwoCube.SetActive(true);
            enableNightOneCube.SetActive(true);
        }
        else
        {
            Debug.LogError("Invalid night number");
        }
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
