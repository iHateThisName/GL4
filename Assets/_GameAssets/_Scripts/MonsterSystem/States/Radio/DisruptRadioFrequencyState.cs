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
        private float disruptResourceInternal;

        /// <summary>
        /// Caches references to the <see cref="Radio"/> (via the runtime SO reference) and
        /// the monster's <see cref="ResourceSensor"/> for resource depletion during disruption.
        /// </summary>
        /// <param name="owningController">The monster controller that owns this state.</param>
        public override void Initialize(MonsterController owningController)
        {
            base.Initialize(owningController);
            this.radio = this.radioRef?.Value;
            this.resourceSensor = this.controller.GetSensor<ResourceSensor>();
        }

        /// <summary>
        /// Starts the disruption timer, subscribes to radio channel-change events to detect when
        /// the player fixes the radio, immediately forces a random non-safe channel, and resets
        /// the disruption and resource-depletion interval accumulators.
        /// </summary>
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
            this.disruptResourceInternal = this.resourceDepletionInterval;
        }

        /// <summary>
        /// Unsubscribes from radio channel events and disposes the base timer on state exit.
        /// </summary>
        public override void OnStateExit()
        {
            if (this.radio != null)
                this.radio.OnChannelChanged -= OnRadioChannelChanged;

            base.OnStateExit(); // Disposes the timer
        }

        /// <summary>
        /// Called every timer tick. Re-disrupts the radio at each <c>disruptionInterval</c> boundary
        /// and depletes the monster's resource at each <c>resourceDepletionInterval</c> boundary.
        /// </summary>
        protected override void OnTimerTick()
        {
            if (this.GetTime() >= this.disruptInternalInterval)
            {
                this.disruptInternalInterval += this.disruptionInterval;
                DisruptChannel();
            }

            if (this.GetTime() >= this.disruptResourceInternal)
            {
                this.disruptResourceInternal += this.resourceDepletionInterval;
                ReduceResource(this.resourceDepletionValue);
            }
        }

        /// <summary>
        /// Subtracts the specified amount from the monster's resource sensor,
        /// modelling the resource cost of sustained disruption.
        /// </summary>
        /// <param name="amount">The positive value to subtract from the resource.</param>
        private void ReduceResource(float amount)
        {
            if (this.resourceSensor != null)
                this.resourceSensor.ModValue(-amount);
        }

        /// <summary>
        /// Forces the radio to a random non-safe channel. Does nothing if no radio is cached.
        /// </summary>
        private void DisruptChannel()
        {
            if (this.radio == null) return;

            // Pick a random channel that isn't the safe channel
            int newChannel = GetRandomNonSafeChannel();
            this.radio.SetChannel(newChannel);
        }

        /// <summary>
        /// Returns a random channel index (1-indexed) that is not the safe channel.
        /// Falls back to channel 1 if there is only one channel available.
        /// Caps retry attempts at 20 to avoid an infinite loop.
        /// </summary>
        /// <returns>A 1-indexed channel number that differs from the safe channel.</returns>
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

        /// <summary>
        /// Callback fired when the player changes the radio channel. Exits this state when the
        /// player successfully restores the safe channel, giving control back to the exit state.
        /// </summary>
        /// <param name="channel">The new channel (1-indexed).</param>
        /// <param name="isSafe">True if the player tuned to the safe channel.</param>
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
