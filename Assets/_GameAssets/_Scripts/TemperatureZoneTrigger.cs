using UnityEngine;

public class TemperatureZoneTrigger : MonoBehaviour {

    [SerializeField] private PlayerTemperatureSimulator.EnumLocationType locationType;

    [ContextMenu("Debug Trigger Zone")]
    public void DebugTrigger() {
        Debug.Log($"Debug Trigger Activated for {locationType} zone.");
        UpdateTemperatureZone(this.locationType);
    }
    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) return;

        if (this.locationType == PlayerTemperatureSimulator.EnumLocationType.Warm && PlayerTemperatureSimulator.Instance.CurrentLocationType == PlayerTemperatureSimulator.EnumLocationType.Cold) {
            return; // Prevent entering warm zone if currently in cold zone because heat zone can reach through walls
        }

        UpdateTemperatureZone(this.locationType);
    }

    private void OnTriggerExit(Collider other) {
        if (!other.CompareTag("Player")) return;

        if (PlayerTemperatureSimulator.Instance.CurrentLocationType == PlayerTemperatureSimulator.EnumLocationType.Warm) {
            UpdateTemperatureZone(PlayerTemperatureSimulator.EnumLocationType.Normal);
        }

        if (PlayerTemperatureSimulator.Instance.CurrentLocationType == PlayerTemperatureSimulator.EnumLocationType.Normal) {
            UpdateTemperatureZone(PlayerTemperatureSimulator.EnumLocationType.Cold);
        }
    }

    private void UpdateTemperatureZone(PlayerTemperatureSimulator.EnumLocationType type) {
        switch (type) {
            case PlayerTemperatureSimulator.EnumLocationType.Cold:
                Debug.Log("Player entered COLD temperature zone.");

                // TODO Apply cold vision

                // Remove heating vision
                //GameManager.Instance.FireAdaptationController.RemoveVolume();
                break;
            case PlayerTemperatureSimulator.EnumLocationType.Warm:
                Debug.Log("Player entered WARM temperature zone.");

                // Apply heating vision
                //GameManager.Instance.FireAdaptationController.ApplyVolume();
                break;
            case PlayerTemperatureSimulator.EnumLocationType.Normal:
                Debug.Log("Player entered NORMAL temperature zone.");

                // Remove heating vision
                //GameManager.Instance.FireAdaptationController.RemoveVolume();
                break;
        }

        PlayerTemperatureSimulator.Instance.SetLocationType(type);
    }
}
