using TMPro;
using UnityEngine;
using static PlayerTemperatureSimulator;

    /// <summary>
    /// Manages the player's UI display
    /// </summary>
public class PlayerUI : MonoBehaviour {

    [Header("Refrences")]
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

    /// <summary>
    /// Handles body temperature state changes by updating the UI text and color.
    /// </summary>
    /// <param name="change">The body temperature state change data.</param>
    private void HandleTemperatureChanged(BodyTemperatureStateChange change) {
        this.temperatureText.text = $"Temperature State: {change.CurrentState}, Location: {PlayerTemperatureSimulator.Instance.CurrentLocationType}";
        UpdateColor(change.CurrentState);
    }

    /// <summary>
    /// Updates the temperature text color based on the current body temperature state.
    /// </summary>
    /// <param name="state">The current body temperature state.</param>
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
