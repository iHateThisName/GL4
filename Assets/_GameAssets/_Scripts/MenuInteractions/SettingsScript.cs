using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class SettingsScript : MonoBehaviour
{
    public bool snapEnabled = false;

    public static SettingsScript instance;

    private void Awake()
    {
        if(instance = null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
        DontDestroyOnLoad(this);
    }

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
