using UnityEngine;

namespace MonsterSystem
{
    public class RadioSensor : MonsterSensor
    {
        [Header("References")]
        [SerializeField] private SO_RadioRef radioRef;

        [Header("State Transitions")]
        [Tooltip("State to transition to when radio is on a dangerous (non-safe) channel")]
        [SerializeField] private MonsterState frenzyState;
        [Tooltip("State to transition to when radio returns to the safe channel")]
        [SerializeField] private MonsterState normalState;

        [Header("Aggression Settings")]
        [Tooltip("Aggression multiplier when on the safe channel")]
        [SerializeField] private float safeAggressionModifier = 0.5f;
        [Tooltip("Aggression multiplier when NOT on the safe channel")]
        [SerializeField] private float dangerAggressionModifier = 1.5f;

        /// <summary>Whether the radio is currently on a dangerous (non-safe) channel</summary>
        public bool IsDangerMode { get; private set; }

        /// <summary>Current channel (1-indexed)</summary>
        public int CurrentChannel { get; private set; }

        /// <summary>Current aggression modifier based on radio state</summary>
        public float CurrentAggressionModifier { get; private set; } = 1f;

        private Radio radio => this.radioRef?.Value;

        protected override void Subscribe()
        {
            if (radio != null)
            {
                radio.OnChannelChanged += OnRadioChannelChanged;
                CurrentChannel = radio.CurrentChannel;
            }
        }

        protected override void Unsubscribe()
        {
            if (radio != null)
                radio.OnChannelChanged -= OnRadioChannelChanged;
        }

        private void OnRadioChannelChanged(int channel, bool isSafe)
        {
            CurrentChannel = channel;
            bool wasDanger = IsDangerMode;
            IsDangerMode = !isSafe;
            CurrentAggressionModifier = IsDangerMode ? dangerAggressionModifier : safeAggressionModifier;

            // Transition to frenzy state when entering danger mode
            if (IsDangerMode && !wasDanger && frenzyState != null)
            {
                TriggerTransitionTo(frenzyState);
            }
            // Transition to normal state when returning to safe
            else if (!IsDangerMode && wasDanger && normalState != null)
            {
                TriggerTransitionTo(normalState);
            }
        }
    }
}
