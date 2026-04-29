using UnityEngine;

namespace MonsterSystem
{
    /// <summary>
    /// Monster sensor that tracks whether the player's <see cref="Radio"/> is on a safe or
    /// dangerous channel. Subscribes to <see cref="Radio.OnChannelChanged"/> and exposes
    /// <see cref="IsDangerMode"/> for use in monster transition conditions.
    /// Automatically triggers a transition to <c>normalState</c> when the player tunes back
    /// to the safe channel while the monster is in danger mode.
    /// </summary>
    public class RadioSensor : MonsterSensor
    {
        [Header("References")]
        [SerializeField] private SO_RadioRef radioRef;

        [Header("State Transitions")]
        [Tooltip("State to transition to when radio returns to the safe channel")]
        [SerializeField] private MonsterState normalState;

        /// <summary>Whether the radio is currently on a dangerous (non-safe) channel</summary>
        public bool IsDangerMode { get; private set; }

        /// <summary>Current channel (1-indexed)</summary>
        public int CurrentChannel { get; private set; }

        private Radio radio => this.radioRef?.Value;

        /// <summary>
        /// Subscribes to the radio's channel-change event and snapshots the current channel.
        /// Called by the base sensor when the monster is initialized.
        /// </summary>
        protected override void Subscribe()
        {
            if (radio != null)
            {
                radio.OnChannelChanged += OnRadioChannelChanged;
                CurrentChannel = radio.CurrentChannel;
            }
        }

        /// <summary>
        /// Unsubscribes from the radio's channel-change event.
        /// Called by the base sensor when the monster is destroyed or disabled.
        /// </summary>
        protected override void Unsubscribe()
        {
            if (radio != null)
                radio.OnChannelChanged -= OnRadioChannelChanged;
        }

        /// <summary>
        /// Updates <see cref="IsDangerMode"/> and <see cref="CurrentChannel"/> when the
        /// radio channel changes. Triggers a transition to <c>normalState</c> if the player
        /// restores the safe channel while the monster was in danger mode.
        /// </summary>
        /// <param name="channel">The new channel (1-indexed).</param>
        /// <param name="isSafe">True if the new channel is the designated safe channel.</param>
        private void OnRadioChannelChanged(int channel, bool isSafe)
        {
            CurrentChannel = channel;
            bool wasDanger = IsDangerMode;
            IsDangerMode = !isSafe;
            
            // Transition to normal state when returning to safe
            if (!IsDangerMode && wasDanger && normalState != null)
            {
                TriggerTransitionTo(normalState);
            }
        }
    }
}
