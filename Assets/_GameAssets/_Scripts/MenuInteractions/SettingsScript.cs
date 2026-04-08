using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class SettingsScript : MonoBehaviour
{
    [SerializeField]
    private GameObject enableSnapCube;
    [SerializeField]
    private GameObject disableSnapCube;

    [SerializeField]
    private GameObject enableTunnelingCube;
    [SerializeField]
    private GameObject disableTunnelingCube;

    [SerializeField]
    private ControllerInputActionManager controllerRefrence;

    public void EnableSnapTurn()
    {
        controllerRefrence.smoothTurnEnabled = false;
        enableSnapCube.SetActive(false);
        disableSnapCube.SetActive(true);
    }

    public void DisableSnapTurn()
    {
        controllerRefrence.smoothTurnEnabled = true;
        disableSnapCube.SetActive(false);
        enableSnapCube.SetActive(true);
    }

    public void EnableTunneling()
    {
        enableTunnelingCube.SetActive(false);
        disableTunnelingCube.SetActive(true);
    }

    public void DisableTunneling()
    {
        disableTunnelingCube.SetActive(false);
        enableTunnelingCube.SetActive(true);
    }
}
