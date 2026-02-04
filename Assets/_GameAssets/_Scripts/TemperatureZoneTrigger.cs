using UnityEngine;

public class TemperatureZoneTrigger : MonoBehaviour {

    [SerializeField] private PlayerTemperatureSimulator.EnumLocationType locationType;

    [ContextMenu("Debug Trigger Zone")]
    public void DebugTrigger() {
        Debug.Log($"Debug Trigger Activated for {locationType} zone.");
        UpdateTemperatureZone();
    }
    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) return;
        UpdateTemperatureZone();
    }

    private void UpdateTemperatureZone() {
        switch (locationType) {
            case PlayerTemperatureSimulator.EnumLocationType.Cold:
                Debug.Log("Player entered COLD temperature zone.");

                // TODO Apply cold vision

                // Remove heating vision
                GameManager.Instance.FireAdaptationController.RemoveVolume();
                break;
            case PlayerTemperatureSimulator.EnumLocationType.Warm:
                Debug.Log("Player entered WARM temperature zone.");

                // Apply heating vision
                GameManager.Instance.FireAdaptationController.ApplyVolume();
                break;
            case PlayerTemperatureSimulator.EnumLocationType.Normal:
                Debug.Log("Player entered NORMAL temperature zone.");

                // Remove heating vision
                GameManager.Instance.FireAdaptationController.RemoveVolume();
                break;
        }

        PlayerTemperatureSimulator.Instance.SetLocationType(locationType);
    }
}
