using Assets.Scripts.Singleton;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class SettingsScript : PersistenSingleton<SettingsScript>
{
    public bool snapEnabled = false;
    public bool tunnelingEnabled = false;
    public bool teleportEnabled = false;

    [SerializeField] private GameObject enableSnapCube;
    [SerializeField] private GameObject disableSnapCube;

    [SerializeField] private GameObject enableTunnelingCube;
    [SerializeField] private GameObject disableTunnelingCube;

    [SerializeField] private GameObject enableTelportCube;
    [SerializeField] private GameObject disableTeleportCube;

    [SerializeField] private GameObject enableNightOneCube;
    [SerializeField] private GameObject enableNightTwoCube;

    [SerializeField] private ControllerInputActionManager rightControllerRefrence;
    [SerializeField] private ControllerInputActionManager leftControllerRefrence;

    [SerializeField] private GameObject tunnelingVignette;

    [SerializeField] private SO_NightSettings nightSettings;

    private void Start()
    {
        tunnelingVignette.SetActive(false);

        nightSettings.SetDebugStartNight(1);
    }

    public void EnableSnapTurn()
    {
        rightControllerRefrence.smoothTurnEnabled = false;
        snapEnabled = true;
        enableSnapCube.SetActive(false);
        disableSnapCube.SetActive(true);
    }

    public void DisableSnapTurn()
    {
        rightControllerRefrence.smoothTurnEnabled = true;
        snapEnabled = false;
        disableSnapCube.SetActive(false);
        enableSnapCube.SetActive(true);
    }

    public void EnableTunneling()
    {
        tunnelingVignette.SetActive(true);
        tunnelingEnabled = true;
        enableTunnelingCube.SetActive(false);
        disableTunnelingCube.SetActive(true);
    }

    public void DisableTunneling()
    {
        tunnelingVignette.SetActive(false);
        tunnelingEnabled = false;
        enableTunnelingCube.SetActive(false);
        disableTunnelingCube.SetActive(true);
    }

    public void EnableTeleport()
    {
        leftControllerRefrence.smoothMotionEnabled = false;
        teleportEnabled = true;
        enableTelportCube.SetActive(false);
        disableTeleportCube.SetActive(true);
    }

    public void DisableTeleport()
    {
        leftControllerRefrence.smoothMotionEnabled = true;
        teleportEnabled = false;
        disableTeleportCube.SetActive(false);
        enableTelportCube.SetActive(true);
    }

    public void EnableNightOne()
    {
        nightSettings.SetDebugStartNight(1);
        enableNightOneCube.SetActive(false);
        enableNightTwoCube.SetActive(true);
    }

    public void EnableNightTwo()
    {
        nightSettings.SetDebugStartNight(2);
        enableNightTwoCube.SetActive(false);
        enableNightOneCube.SetActive(true);
    }
}
