using UnityEngine;

namespace MonsterSystem
{
    /// <summary>
    /// Drains satiety over time and triggers state transitions at thresholds.
    /// The drain rate is influenced by night-specific overrides, the radio sensor,
    /// and an external drain multiplier.
    /// </summary>
    public class SatietySensor : MonsterSensor
    {
        [Header("=== Satiety Configuration ===")]
        [SerializeField] private float maxSatiety = 60f;        // Upper bound for satiety value
        [SerializeField] private float baseDrainRate = 0.4f;     // Base amount of satiety drained per second
        [SerializeField] private float hungryThreshold = 40f;    // Satiety level at which the hungry state triggers
        [SerializeField] private float angryThreshold = 20f;     // Satiety level at which the angry state triggers
        [SerializeField] private float killThreshold = 0f;       // Satiety level at which the kill state triggers

        [Header("=== State Transitions ===")]
        [SerializeField] private MonsterState hungryState; // State to enter when satiety drops below the hungry threshold
        [SerializeField] private MonsterState angryState;  // State to enter when satiety drops below the angry threshold
        [SerializeField] private MonsterState killState;   // State to enter when satiety drops below the kill threshold

        private float rate;                          // Computed drain rate for the current tick (base * modifiers)
        private MonsterState lastTriggeredState;     // Tracks the last state we triggered to avoid duplicate transitions

        /// <summary>
        /// The monster's current satiety value, clamped between 0 and maxSatiety.
        /// </summary>
        public float CurrentSatiety { get; private set; }

        /// <summary>
        /// External multiplier applied to the drain rate each tick.
        /// Set by other systems to speed up or slow down satiety loss.
        /// </summary>
        public float DrainMultiplier { get; set; } = 1f;

        /// <summary>
        /// Initializes the sensor and sets satiety to its maximum value.
        /// </summary>
        /// <param name="owningMonster">The monster controller that owns this sensor.</param>
        public override void Initialize(MonsterController owningMonster)
        {
            base.Initialize(owningMonster);
            this.CurrentSatiety = this.maxSatiety;
        }

        /// <summary>
        /// Called each sensor tick. Computes the effective drain rate from all modifiers,
        /// drains satiety, and checks for state transition thresholds.
        /// </summary>
        /// <param name="tickDelta">Time elapsed since the last tick.</param>
        public override void OnTick(float tickDelta)
        {
            base.OnTick(tickDelta);

            // Start with the base drain rate
            this.rate = this.baseDrainRate;

            // Apply night-specific aggression multiplier from configuration
            if (this.controller.Config != null)
            {
                var nightOverride = this.controller.Config.GetOverrideForNight(this.controller.CurrentNight);
                this.rate *= nightOverride.aggressionMultiplier;
            }

            // Apply radio sensor aggression modifier if available
            var radioSensor = this.controller.GetSensor<RadioSensor>();
            if (radioSensor != null)
                this.rate *= radioSensor.CurrentAggressionModifier;

            // Apply external drain multiplier
            this.rate *= this.DrainMultiplier;

            // Drain satiety and clamp within valid range
            float previousSatiety = this.CurrentSatiety;
            this.CurrentSatiety = Mathf.Clamp(this.CurrentSatiety - this.rate * tickDelta, 0f, this.maxSatiety);

            // Check if any threshold was crossed and trigger the appropriate state
            this.HandleStateTransitions(previousSatiety);
        }

        /// <summary>
        /// Evaluates satiety thresholds and triggers state transitions when a boundary is crossed.
        /// Kill threshold is checked unconditionally; hungry and angry require crossing the boundary.
        /// </summary>
        /// <param name="previousSatiety">The satiety value before the current tick's drain.</param>
        private void HandleStateTransitions(float previousSatiety)
        {
            // Determine target state based on current satiety
            MonsterState targetState = null;

            // Kill threshold: triggers whenever satiety is at or below zero
            if (this.CurrentSatiety <= this.killThreshold && this.killState != null)
                targetState = this.killState;
            // Angry threshold: only triggers on the tick that crosses the boundary
            else if (this.CurrentSatiety <= this.angryThreshold && previousSatiety > this.angryThreshold && this.angryState != null)
                targetState = this.angryState;
            // Hungry threshold: only triggers on the tick that crosses the boundary
            else if (this.CurrentSatiety <= this.hungryThreshold && previousSatiety > this.hungryThreshold && this.hungryState != null)
                targetState = this.hungryState;

            // Only trigger if we have a new target and it's different from last triggered
            if (targetState != null && targetState != this.lastTriggeredState)
            {
                this.lastTriggeredState = targetState;
                this.TriggerTransitionTo(targetState);
            }
        }

        /// <summary>
        /// Called when the monster's state changes. Resets the last triggered state
        /// so the same threshold can fire again if satiety fluctuates.
        /// </summary>
        public override void OnStateChanged()
        {
            base.OnStateChanged();
            // Reset so we can trigger the same state again if satiety goes back up and down
            this.lastTriggeredState = null;
        }

        /// <summary>
        /// Adds (or subtracts) the given amount to the current satiety, clamped to valid bounds.
        /// </summary>
        /// <param name="amount">The amount to add (positive) or subtract (negative).</param>
        public void AddSatiety(float amount)
        {
            this.CurrentSatiety = Mathf.Clamp(this.CurrentSatiety + amount, 0f, this.maxSatiety);
        }
    }
}
