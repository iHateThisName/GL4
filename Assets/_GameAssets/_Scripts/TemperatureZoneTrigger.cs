using UnityEngine;

public class TemperatureZoneTrigger : MonoBehaviour {

    [SerializeField] private PlayerTemperatureSimulator.EnumLocationType LocationType;

    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) return;
        PlayerTemperatureSimulator.Instance.SetLocationType(LocationType);
    }
}
