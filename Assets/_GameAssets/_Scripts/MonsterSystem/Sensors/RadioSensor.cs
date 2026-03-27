using UnityEngine;

namespace MonsterSystem
{
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
            
            // Transition to normal state when returning to safe
            if (!IsDangerMode && wasDanger && normalState != null)
            {
                TriggerTransitionTo(normalState);
            }
        }
    }
}
