using TMPro;
using UnityEngine;
using static PlayerTemperatureSimulator;

public class PlayerUI : MonoBehaviour {
    [SerializeField] private TMP_Text temperatureText;

    private void Start() {
        HandleTemperatureChanged(new BodyTemperatureStateChange {
            CurrentState = PlayerTemperatureSimulator.Instance.CurrentBodyTemperatureState
        });
    }

    private void OnEnable() {
        PlayerTemperatureSimulator.OnBodyTemperatureStateChanged += HandleTemperatureChanged;
    }

    private void OnDisable() {
        PlayerTemperatureSimulator.OnBodyTemperatureStateChanged -= HandleTemperatureChanged;
    }

    private void HandleTemperatureChanged(BodyTemperatureStateChange change) {
        temperatureText.text = $"Temperature: {change.CurrentState}";
        UpdateColor(change.CurrentState);
    }

    private void UpdateColor(EnumBodyTemperatureState state) {
        switch (state) {
            case EnumBodyTemperatureState.ModerateHypothermia:
                this.temperatureText.color = Color.blue;
                break;

            case EnumBodyTemperatureState.MildHypothermia:
                this.temperatureText.color = Color.cyan;
                break;

            case EnumBodyTemperatureState.Normal:
                this.temperatureText.color = Color.green;
                break;

            case EnumBodyTemperatureState.MildHyperthermia:
                this.temperatureText.color = Color.yellow;
                break;

            case EnumBodyTemperatureState.ModerateHyperthermia:
                this.temperatureText.color = Color.red;
                break;

            case EnumBodyTemperatureState.Hyperthermia:
            case EnumBodyTemperatureState.Hypothermia:
                this.temperatureText.color = Color.magenta; // Extreme conditions Dead.
                break;
        }
    }
}
