using UnityEngine;

namespace MonsterSystem
{
    /// <summary>
    /// Disrupts the radio by changing to random non-safe channels at intervals.
    /// Exits when the player sets the radio back to the safe channel.
    /// </summary>
    public class DisruptRadioFrequencyState : MonsterStateWithTimer
    {
        [Header("References")]
        [SerializeField] private SO_RuntimeReferences runtimeReferences;

        [Header("Disruption Settings")]
        [Tooltip("State to transition to when radio is set back to safe channel")]
        [SerializeField] private MonsterState exitState;

        private Radio radio;

        public override void Initialize(MonsterController owningController)
        {
            base.Initialize(owningController);
            radio = runtimeReferences?.Radio;
        }

        public override void OnStateEnter()
        {
            base.OnStateEnter(); // Starts the timer
            
            if (radio == null)
            {
                Debug.LogWarning("[DisruptRadioFrequencyState] No Radio found!", this);
                return;
            }

            // Subscribe to channel changes to detect when player fixes the radio
            radio.OnChannelChanged += OnRadioChannelChanged;

            // Immediately disrupt to a random non-safe channel
            DisruptChannel();
        }

        public override void OnStateExit()
        {
            if (radio != null)
                radio.OnChannelChanged -= OnRadioChannelChanged;

            base.OnStateExit(); // Disposes the timer
        }

        protected override void OnTimerTick()
        {
            DisruptChannel();
        }

        private void DisruptChannel()
        {
            if (radio == null) return;

            // Pick a random channel that isn't the safe channel
            int newChannel = GetRandomNonSafeChannel();
            radio.SetChannel(newChannel);
        }

        private int GetRandomNonSafeChannel()
        {
            int safeChannel = radio.SafeChannel;
            int totalChannels = radio.TotalChannels;

            // If only 1 channel, can't avoid safe
            if (totalChannels <= 1) return 1;

            int newChannel;
            int attempts = 0;
            do
            {
                newChannel = Random.Range(1, totalChannels + 1);
                attempts++;
            }
            while (newChannel == safeChannel && attempts < 20);

            return newChannel;
        }

        private void OnRadioChannelChanged(int channel, bool isSafe)
        {
            // Player set the radio back to safe channel - exit this state
            if (isSafe && exitState != null)
            {
                RequestTransition(exitState);
            }
        }
    }
}
