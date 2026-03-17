using UnityEngine;

namespace MonsterSystem
{
    /// <summary>
    /// Event-driven sensor that tracks the player's location type (Cold, Warm, etc.)
    /// and triggers state transitions based on whether the location is hostile.
    /// Near-zero tick cost: caches a boolean from location change events,
    /// tick is just two comparisons.
    /// </summary>
    public class LocationSensor : MonsterSensor
    {
        [Header("=== Hostile Locations ===")]
        [Tooltip("Location types that cause the monster to pursue the player.")]
        [SerializeField] private PlayerTemperatureSimulator.EnumLocationType[] hostileLocations =
            new[] { PlayerTemperatureSimulator.EnumLocationType.Cold }; // Array of location types considered hostile to the player

        [Header("=== State Transitions ===")]
        [SerializeField] private MonsterState chaseState;  // State to transition to when player is in a hostile location
        [SerializeField] private MonsterState patrolState;  // State to transition to when player is in a safe location

        private bool isHostile; // Cached flag indicating whether the current location is hostile

        /// <summary>
        /// The player's current location type as reported by the temperature simulator.
        /// </summary>
        public PlayerTemperatureSimulator.EnumLocationType CurrentLocationType { get; private set; }

        /// <summary>
        /// Initializes the sensor, reads the initial location from the temperature simulator,
        /// and subscribes to location change events.
        /// </summary>
        /// <param name="owningMonster">The monster controller that owns this sensor.</param>
        public override void Initialize(MonsterController owningMonster)
        {
            base.Initialize(owningMonster);

            // Read initial location from the temperature simulator singleton
            if (PlayerTemperatureSimulator.Instance != null)
            {
                this.CurrentLocationType = PlayerTemperatureSimulator.Instance.CurrentLocationType;
                this.CacheIsHostile(this.CurrentLocationType);
            }

            // Subscribe to location change events for future updates
            PlayerTemperatureSimulator.OnLocationTypeChanged += this.HandleLocationChanged;
        }

        private void OnDestroy()
        {
            // Unsubscribe to prevent memory leaks and null-reference callbacks
            PlayerTemperatureSimulator.OnLocationTypeChanged -= this.HandleLocationChanged;
        }

        /// <summary>
        /// Handles a location type change event from the temperature simulator.
        /// Updates the cached location and recalculates hostility.
        /// </summary>
        /// <param name="newLocation">The new location type the player has entered.</param>
        private void HandleLocationChanged(PlayerTemperatureSimulator.EnumLocationType newLocation)
        {
            this.CurrentLocationType = newLocation;
            this.CacheIsHostile(newLocation);
        }

        /// <summary>
        /// Caches whether the given location is in the hostile locations array.
        /// Uses a simple loop to avoid allocations.
        /// </summary>
        /// <param name="location">The location type to check against the hostile list.</param>
        private void CacheIsHostile(PlayerTemperatureSimulator.EnumLocationType location)
        {
            this.isHostile = false;

            // Iterate through the hostile locations array to check for a match
            for (int i = 0; i < this.hostileLocations.Length; i++)
            {
                if (this.hostileLocations[i] == location)
                {
                    this.isHostile = true;
                    break;
                }
            }
        }

        /// <summary>
        /// Called each sensor tick. Determines the target state based on the cached
        /// hostility flag and triggers a transition if the state has changed.
        /// </summary>
        /// <param name="tickDelta">Time elapsed since the last tick.</param>
        public override void OnTick(float tickDelta)
        {
            base.OnTick(tickDelta);

            // Select target state based on whether the player is in a hostile location
            MonsterState targetState = this.isHostile ? this.chaseState : this.patrolState;

            // Only trigger a transition if a valid target state differs from the current state
            if (targetState != null && targetState != this.controller.CurrentState)
                this.TriggerTransitionTo(targetState);
        }
    }
}
