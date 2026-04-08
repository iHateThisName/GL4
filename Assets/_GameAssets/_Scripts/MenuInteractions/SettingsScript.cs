using Assets.Scripts.Singleton;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class SettingsScript : PersistenSingleton<SettingsScript>
{
    public bool snapEnabled = false;

    [SerializeField]
    private GameObject enableSnapCube;
    [SerializeField]
    private GameObject disableSnapCube;

    [SerializeField]
    private ControllerInputActionManager controllerRefrence;

    public void EnableSnapTurn()
    {
        controllerRefrence.smoothTurnEnabled = false;
        snapEnabled = true;
        enableSnapCube.SetActive(false);
        disableSnapCube.SetActive(true);
    }

    public void DisableSnapTurn()
    {
        controllerRefrence.smoothTurnEnabled = true;
        snapEnabled = false;
        disableSnapCube.SetActive(false);
        enableSnapCube.SetActive(true);
    }
}
