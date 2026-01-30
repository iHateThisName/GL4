using UnityEngine;

public class TemperatureZoneTrigger : MonoBehaviour {

    [SerializeField] private PlayerTemperatureSimulator.EnumLocationType locationType;

    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) return;
        PlayerTemperatureSimulator.Instance.SetLocationType(locationType);
    }
}
