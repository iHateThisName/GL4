using Assets.Scripts.Singleton;
using System;
using UnityEngine;
using static PlayerTemperatureSimulator;

public class PlayerTemperatureSimulator : Singleton<PlayerTemperatureSimulator> {

    [SerializeField] private float currentBodyTemperature = 37.0f; // Normal human body temperature in Celsius
    private readonly float MIN_COMFORTABLE_TEMPERATURE = 35.0f; // Hypothermia threshold,
                                                                // 32 - 35 C is mild hypothermia (shivering, confusion),
                                                                // 28 - 32 C is moderate (slurred speech, drowsiness),
                                                                // below 28 C is severe (unconsciousness, risk of death)

    private readonly float MAX_COMFORTABLE_TEMPERATURE = 39.0f; // Hyperthermia threshold, 
                                                                // 38 - 39 C is mild hyperthermia (heat exhaustion),
                                                                // 39 - 41 C is moderate (heat stroke risk),
                                                                // above 41 C is severe (risk of organ failure)

    private readonly float FREEZE_RATE = -0.15f; // Rate of temperature change per second
    private readonly float NORMAL_RATE = -0.03f; // Rate of temperature change per second
    private readonly float WARM_RATE = 0.12f; // Rate of temperature change per second

    [SerializeField] private EnumLocationType currentLocationType = EnumLocationType.Normal;
    [SerializeField] private EnumBodyTemperatureState currentBodyTemperatureState = EnumBodyTemperatureState.Normal;

    // Event triggered when body temperature state changes
    public static Action<BodyTemperatureStateChange> OnBodyTemperatureStateChanged;

    // Temperature change rate based on location type, Normal slowly decreases, Cold rapidly decreases, Warm increases
    public enum EnumLocationType { Normal, Cold, Warm }
    // Player temperature states based on current body temperature
    public enum EnumBodyTemperatureState { Normal, MildHypothermia, ModerateHypothermia, Hypothermia, MildHyperthermia, ModerateHyperthermia, Hyperthermia }

    /// <summary>
    /// Unity's FixedUpdate method called at fixed time intervals.
    /// Simulates temperature changes and updates the body temperature state.
    /// </summary>
    private void FixedUpdate() {
        // Simulate temperature changes over time
        SimulateTemperatureChange();
        // Update temperature state based on current body temperature
        UpdateBodyTemperatureState();

    }

    /// <summary>
    /// Sets the current location type which affects the rate of temperature change.
    /// Should be called by trigger colliders or area checks when the player enters a new environment.
    /// </summary>
    /// <param name="locationType">The type of location (Normal, Cold, or Warm).</param>
    public void SetLocationType(EnumLocationType locationType) {
        if (this.currentLocationType == locationType) return; // No change in location type
        this.currentLocationType = locationType;
    }

    /// <summary>
    /// Evaluates the current body temperature and updates the temperature state accordingly.
    /// Triggers a notification event if the state has changed from the previous frame.
    /// </summary>
    private void UpdateBodyTemperatureState() {
        EnumBodyTemperatureState previousState = this.currentBodyTemperatureState;

        if (this.currentBodyTemperature < MIN_COMFORTABLE_TEMPERATURE) {
            if (this.currentBodyTemperature >= 32.0f) {
                this.currentBodyTemperatureState = EnumBodyTemperatureState.MildHypothermia;
            } else if (this.currentBodyTemperature >= 28.0f) {
                this.currentBodyTemperatureState = EnumBodyTemperatureState.ModerateHypothermia;
            } else {
                this.currentBodyTemperatureState = EnumBodyTemperatureState.Hypothermia;
            }

        } else if (this.currentBodyTemperature > MAX_COMFORTABLE_TEMPERATURE) {
            if (this.currentBodyTemperature <= 39.0f) {
                this.currentBodyTemperatureState = EnumBodyTemperatureState.MildHyperthermia;
            } else if (this.currentBodyTemperature <= 41.0f) {
                this.currentBodyTemperatureState = EnumBodyTemperatureState.ModerateHyperthermia;
            } else {
                this.currentBodyTemperatureState = EnumBodyTemperatureState.Hyperthermia;
            }
        } else {
            this.currentBodyTemperatureState = EnumBodyTemperatureState.Normal;
        }

        if (previousState != this.currentBodyTemperatureState) {
            NotifyBodyTempetureStateChange(previousState, this.currentBodyTemperatureState);
        }
    }

    /// <summary>
    /// Notifies subscribers about a change in body temperature state.
    /// Logs the state change and invokes the OnBodyTemperatureStateChanged event.
    /// </summary>
    /// <param name="previousState">The previous body temperature state.</param>
    /// <param name="currentState">The new body temperature state.</param>
    private void NotifyBodyTempetureStateChange(EnumBodyTemperatureState previousState, EnumBodyTemperatureState currentState) {
        Debug.Log($"Temperature state changed from {previousState} to {currentState}");
        OnBodyTemperatureStateChanged?.Invoke(new BodyTemperatureStateChange { PreviousState = previousState, CurrentState = currentState });
    }

    /// <summary>
    /// Simulates the change in body temperature over time based on the current location type.
    /// Applies the appropriate temperature change rate multiplied by the fixed delta time.
    /// </summary>
    private void SimulateTemperatureChange() {
        float deltaTemperature = 0f;
        switch (this.currentLocationType) {
            case EnumLocationType.Normal:
                deltaTemperature = this.NORMAL_RATE; // Normal locations cause slight cooling
                break;
            case EnumLocationType.Cold:
                deltaTemperature = this.FREEZE_RATE; // Cold locations cause rapid cooling
                break;
            case EnumLocationType.Warm:
                deltaTemperature = this.WARM_RATE; // Warm locations cause warming
                break;
        }
        // Update body temperature
        this.currentBodyTemperature += deltaTemperature * Time.fixedDeltaTime;
    }
}

public struct BodyTemperatureStateChange {
    public PlayerTemperatureSimulator.EnumBodyTemperatureState PreviousState;
    public PlayerTemperatureSimulator.EnumBodyTemperatureState CurrentState;
}
