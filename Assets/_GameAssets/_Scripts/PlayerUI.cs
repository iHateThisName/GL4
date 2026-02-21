using TMPro;
using UnityEngine;
using static PlayerTemperatureSimulator;

    /// <summary>
    /// Manages the player's UI display
    /// </summary>
public class PlayerUI : MonoBehaviour {

    [Header("Refrences")]
    [SerializeField] private TMP_Text temperatureText;
    [SerializeField] private TMP_Text locationText;
    [SerializeField] private TextMeshProUGUI hungerText;

    private bool useDebugInfo = false;

    private void Awake() {
#if UNITY_EDITOR
        this.useDebugInfo = true;
#endif
    }
    private void Start() {
        if (!this.useDebugInfo) return; // Avoid initializing if we're not in debug mode.

        HandleTemperatureChanged(new BodyTemperatureStateChange {
            CurrentState = PlayerTemperatureSimulator.Instance.CurrentBodyTemperatureState
        });
        HandleHungerChanged(100);
        HandleLocationChanged(PlayerTemperatureSimulator.Instance.CurrentLocationType);

    }

    private void OnEnable() {
        this.temperatureText.text = "";
        this.hungerText.text = "";
        this.locationText.text = "";
        if (!this.useDebugInfo) return; // Avoid subscribing if we're not in debug mode.

        PlayerTemperatureSimulator.OnBodyTemperatureStateChanged += HandleTemperatureChanged;
        PlayerTemperatureSimulator.OnLocationTypeChanged += HandleLocationChanged;
        HungerSystem.OnHungerChanged += HandleHungerChanged;
    }

    private void OnDisable() {
        if (!this.useDebugInfo) return; // Avoid unsubscribing if we never subscribed in the first place.

        PlayerTemperatureSimulator.OnBodyTemperatureStateChanged -= HandleTemperatureChanged;
        PlayerTemperatureSimulator.OnLocationTypeChanged -= HandleLocationChanged;
        HungerSystem.OnHungerChanged -= HandleHungerChanged;
    }

    /// <summary>
    /// Handles body temperature state changes by updating the UI text and color.
    /// </summary>
    /// <param name="change">The body temperature state change data.</param>
    private void HandleTemperatureChanged(BodyTemperatureStateChange change) {
        this.temperatureText.text = $"Temperature State: {change.CurrentState}";
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
    
    private void HandleHungerChanged(float hunger) {
        this.hungerText.text = "Hunger: " + hunger.ToString("F2");
    }
    private void HandleLocationChanged(EnumLocationType type) {
        this.locationText.text = $"Location: {PlayerTemperatureSimulator.Instance.CurrentLocationType}";
    }
}
