using Assets.Scripts.Singleton;
using System;
using UnityEngine;

public class PlayerTemperatureSimulator : Singleton<PlayerTemperatureSimulator> {

    [SerializeField] private float currentBodyTemperature = 37.0f; // Normal human body temperature in Celsius
    private readonly float MIN_COMFORTABLE_TEMPERATURE = 35.2f; // Hypothermia threshold,
                                                                // 32 - 35 C is mild hypothermia (shivering, confusion),
                                                                // 28 - 32 C is moderate (slurred speech, drowsiness),
                                                                // below 28 C is severe (unconsciousness, risk of death)

    private readonly float MAX_COMFORTABLE_TEMPERATURE = 38.8f; // Hyperthermia threshold, 
                                                                // 38 - 39 C is mild hyperthermia (heat exhaustion),
                                                                // 39 - 41 C is moderate (heat stroke risk),
                                                                // above 41 C is severe (risk of organ failure)

    private readonly float FREEZE_RATE = -0.15f; // Rate of temperature change per second outside in cold
    private readonly float NORMAL_RATE = -0.03f; // Rate of temperature change per second inside
    private readonly float WARM_RATE = 0.12f; // Rate of temperature change per second while next to fireplace

    [SerializeField] private EnumLocationType currentLocationType = EnumLocationType.Normal;
    [SerializeField] private EnumBodyTemperatureState currentBodyTemperatureState = EnumBodyTemperatureState.Normal;
    public EnumBodyTemperatureState CurrentBodyTemperatureState => this.currentBodyTemperatureState;
    public EnumLocationType CurrentLocationType => this.currentLocationType;

    // Event triggered when body temperature state changes
    public static Action<BodyTemperatureStateChange> OnBodyTemperatureStateChanged;
    // Event triggered when location type changes.
    public static Action<EnumLocationType> OnLocationTypeChanged;

    // Temperature change rate based on location type, Normal slowly decreases, Cold rapidly decreases, Warm increases
    public enum EnumLocationType { Normal, Cold, Warm, Shack }
    // Player temperature states based on current body temperature
    public enum EnumBodyTemperatureState {
        /// <summary>
        /// Normal body temperature (35-39�C). Player feels comfortable.
        /// </summary>
        Normal,

        /// <summary>
        /// Mild hypothermia (32-35�C). Player feels cold and may experience shivering and confusion.
        /// </summary>
        MildHypothermia,

        /// <summary>
        /// Moderate hypothermia (28-32�C). Player feels very cold with slurred speech and drowsiness.
        /// </summary>
        ModerateHypothermia,

        /// <summary>
        /// Severe hypothermia (below 28�C). Player is frozen with risk of unconsciousness and death.
        /// </summary>
        Hypothermia,

        /// <summary>
        /// Mild hyperthermia (38-39�C). Player feels hot and may experience heat exhaustion.
        /// </summary>
        MildHyperthermia,

        /// <summary>
        /// Moderate hyperthermia (39-41�C). Player feels very hot with heat stroke risk.
        /// </summary>
        ModerateHyperthermia,

        /// <summary>
        /// Severe hyperthermia (above 41�C). Player is overheating with risk of organ failure.
        /// </summary>
        Hyperthermia
    }

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
        OnLocationTypeChanged?.Invoke(locationType);
    }

    /// <summary>
    /// Evaluates the current body temperature and updates the temperature state accordingly.
    /// Triggers a notification event if the state has changed from the previous frame.
    /// </summary>
    private void UpdateBodyTemperatureState() {
        EnumBodyTemperatureState previousState = this.currentBodyTemperatureState;
        this.currentBodyTemperatureState = GetStateFromTemperature(this.currentBodyTemperature);

        if (previousState != this.currentBodyTemperatureState) {
            NotifyBodyTempetureStateChange(previousState, this.currentBodyTemperatureState);
        }
    }

    /// <summary>
    /// Determines the body temperature state based on the specified temperature value.
    /// </summary>
    /// <remarks>Temperature thresholds are based on typical clinical definitions for hypothermia and
    /// hyperthermia. Use this method to classify body temperature readings into medically relevant states.</remarks>
    /// <param name="temp">The body temperature, in degrees Celsius, to evaluate.</param>
    /// <returns>A value of the EnumBodyTemperatureState enumeration that represents the state corresponding to the specified
    /// temperature.</returns>
    private EnumBodyTemperatureState GetStateFromTemperature(float temp) {
        if (temp < 28f) return EnumBodyTemperatureState.Hypothermia;
        if (temp < 32f) return EnumBodyTemperatureState.ModerateHypothermia;
        if (temp < MIN_COMFORTABLE_TEMPERATURE) return EnumBodyTemperatureState.MildHypothermia;

        if (temp > 41f) return EnumBodyTemperatureState.Hyperthermia;
        if (temp > 39f) return EnumBodyTemperatureState.ModerateHyperthermia;
        if (temp > MAX_COMFORTABLE_TEMPERATURE) return EnumBodyTemperatureState.MildHyperthermia;

        return EnumBodyTemperatureState.Normal;
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

        if (currentState == EnumBodyTemperatureState.Hypothermia || currentState == EnumBodyTemperatureState.Hyperthermia)
            DeathSystem.KillPlayer(DeathSystem.DeathEvent.DeathReason.Temperature, "", false);
    }

    /// <summary>
    /// Simulates the change in body temperature over time based on the current location type.
    /// Applies the appropriate temperature change rate multiplied by the fixed delta time.
    /// </summary>
    private void SimulateTemperatureChange() {
        // Calculate next body temperature
        float nextTemp = this.currentBodyTemperature + GetLocationRate(this.currentLocationType) * Time.fixedDeltaTime;

        // Get location-specific min and max temperatures
        float minTemp = GetLocationMinTemp(this.currentLocationType);
        float maxTemp = GetLocationMaxTemp(this.currentLocationType);

        // Clamp temperature to location-specific min/max (setting a max and min value for temp)
        if (nextTemp < this.currentBodyTemperature && this.currentBodyTemperature <= minTemp) {
            nextTemp = currentBodyTemperature; // stop cooling because we've reached min for this location

        } else if (nextTemp > this.currentBodyTemperature && this.currentBodyTemperature >= maxTemp) {
            nextTemp = currentBodyTemperature; // stop heating because we've reached max for this location
        }

        // Update body temperature, will be no change if clamped
        this.currentBodyTemperature = nextTemp;
    }

    private float GetLocationRate(EnumLocationType location) {
        // The rate of temperature change in different environments
        return location switch {
            EnumLocationType.Cold => this.FREEZE_RATE, // Cold locations cause rapid cooling
            EnumLocationType.Normal => this.NORMAL_RATE, // Normal locations cause slight cooling
            EnumLocationType.Warm => this.WARM_RATE, // Warm locations cause warming
            EnumLocationType.Shack => this.FREEZE_RATE,
            _ => this.NORMAL_RATE,
        };
    }

    private float GetLocationMinTemp(EnumLocationType location) {
        // The lowest allowed body temperature in different environments
        return location switch {
            EnumLocationType.Cold => 25f,
            EnumLocationType.Normal => 25f,
            EnumLocationType.Warm => 36f,
            _ => 25f,
        };
    }

    private float GetLocationMaxTemp(EnumLocationType location) {
        // The highest allowed body temperature in different environments
        return location switch {
            EnumLocationType.Cold => 36f,
            EnumLocationType.Normal => 37f,
            EnumLocationType.Warm => 40.2f,
            _ => 41f,
        };
    }
}

public struct BodyTemperatureStateChange {
    public PlayerTemperatureSimulator.EnumBodyTemperatureState PreviousState;
    public PlayerTemperatureSimulator.EnumBodyTemperatureState CurrentState;
}
