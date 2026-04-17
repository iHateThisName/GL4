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
        [SerializeField] private SO_RadioRef radioRef;

        [Header("Disruption Settings")]
        [Tooltip("State to transition to when radio is set back to safe channel")]
        [SerializeField] private MonsterState exitState;
        
        [SerializeField] private float disruptionInterval = 5f;
        [SerializeField] private float resourceDepletionValue = 2.5f;
        [SerializeField] private float resourceDepletionInterval = 10f;

        private Radio radio;
        private ResourceSensor resourceSensor;
        private float disruptInternalInterval;
        private float disruptResourceInterntal;

        public override void Initialize(MonsterController owningController)
        {
            base.Initialize(owningController);
            this.radio = this.radioRef?.Value;
            this.resourceSensor = this.controller.GetSensor<ResourceSensor>();
        }

        public override void OnStateEnter()
        {
            base.OnStateEnter(); // Starts the timer
            
            if (this.radio == null)
            {
                Debug.LogWarning("[DisruptRadioFrequencyState] No Radio found!", this);
                return;
            }

            // Subscribe to channel changes to detect when player fixes the radio
            this.radio.OnChannelChanged += OnRadioChannelChanged;

            // Immediately disrupt to a random non-safe channel
            DisruptChannel();
            
            this.disruptInternalInterval = this.disruptionInterval;
            this.disruptResourceInterntal = this.resourceDepletionInterval;
        }

        public override void OnStateExit()
        {
            if (this.radio != null)
                this.radio.OnChannelChanged -= OnRadioChannelChanged;

            base.OnStateExit(); // Disposes the timer
        }

        protected override void OnTimerTick()
        {
            if (this.GetTime() >= this.disruptInternalInterval)
            {
                this.disruptInternalInterval += this.disruptionInterval;
                DisruptChannel();
            }

            if (this.GetTime() >= this.disruptResourceInterntal)
            {
                this.disruptResourceInterntal += this.resourceDepletionInterval;
                ReduceResource(this.resourceDepletionValue);
            }
        }

        private void ReduceResource(float amount)
        {
            if (this.resourceSensor != null)
                this.resourceSensor.ModValue(-amount);
        }

        private void DisruptChannel()
        {
            if (this.radio == null) return;

            // Pick a random channel that isn't the safe channel
            int newChannel = GetRandomNonSafeChannel();
            this.radio.SetChannel(newChannel);
        }

        private int GetRandomNonSafeChannel()
        {
            int safeChannel = this.radio.SafeChannel;
            int totalChannels = this.radio.TotalChannels;

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
            if (isSafe && this.exitState != null)
            {
                RequestTransition(this.exitState);
            }
        }
    }
}
