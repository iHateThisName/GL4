using System;
using UnityEngine;

namespace MonsterSystem
{
    /// <summary>
    /// Sensor that monitors the distance between the monster and the player,
    /// raising events when the player enters or exits the detection range.
    /// </summary>
    public class PlayerProximitySensor : MonsterSensor
    {
        [SerializeField] private float detectionRange = 10f; // Maximum distance at which the player is considered "in range"

        /// <summary>
        /// Whether the player is currently within the detection range.
        /// </summary>
        public bool IsPlayerInRange { get; private set; }

        /// <summary>
        /// The current world-space distance from this sensor to the player.
        /// Defaults to float.MaxValue when no player is present.
        /// </summary>
        public float DistanceToPlayer { get; private set; } = float.MaxValue;

        /// <summary>
        /// Convenience accessor for the player's transform from the monster configuration.
        /// </summary>
        public Transform PlayerTransform => this.controller.Config.PlayerTarget;

        /// <summary>
        /// Raised once when the player first enters the detection range.
        /// </summary>
        public event Action OnPlayerEntered; // Event fired on range entry

        /// <summary>
        /// Raised once when the player first exits the detection range.
        /// </summary>
        public event Action OnPlayerExited; // Event fired on range exit

        /// <summary>
        /// Called each sensor tick. Calculates the distance to the player and
        /// raises enter/exit events when the player crosses the detection boundary.
        /// </summary>
        /// <param name="tickDelta">Time elapsed since the last tick.</param>
        public override void OnTick(float tickDelta)
        {
            base.OnTick(tickDelta);

            // Retrieve the player transform; bail out if unavailable
            Transform player = this.PlayerTransform;
            if (player == null) return;

            // Calculate the current distance from the monster to the player
            this.DistanceToPlayer = Vector3.Distance(this.transform.position, player.position);

            // Store previous state for edge detection
            bool wasInRange = this.IsPlayerInRange;
            this.IsPlayerInRange = this.DistanceToPlayer <= this.detectionRange;

            // Fire entry event when player transitions from out-of-range to in-range
            if (this.IsPlayerInRange && !wasInRange)
                this.OnPlayerEntered?.Invoke();
            // Fire exit event when player transitions from in-range to out-of-range
            else if (!this.IsPlayerInRange && wasInRange)
                this.OnPlayerExited?.Invoke();
        }
    }
}
