using UnityEngine;

public class TemperatureZoneTrigger : MonoBehaviour {

    [SerializeField] private PlayerTemperatureSimulator.EnumLocationType locationType;

    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) return;
        TemperatureZoneManager.Instance.EnterZone(this.locationType);

        if (this.locationType == PlayerTemperatureSimulator.EnumLocationType.Normal) {
            TemperatureZoneManager.Instance.ExitZone(PlayerTemperatureSimulator.EnumLocationType.Cold);
        }

    }

    private void OnTriggerExit(Collider other) {
        if (!other.CompareTag("Player")) return;
        TemperatureZoneManager.Instance.ExitZone(this.locationType);

        if (this.locationType == PlayerTemperatureSimulator.EnumLocationType.Normal) {
            TemperatureZoneManager.Instance.EnterZone(PlayerTemperatureSimulator.EnumLocationType.Cold);
        }
    }


}
