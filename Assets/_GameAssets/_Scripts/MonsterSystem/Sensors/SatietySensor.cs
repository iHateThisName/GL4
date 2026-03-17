using UnityEngine;

namespace MonsterSystem
{
    /// Drains satiety over time and triggers state transitions at thresholds.
    public class SatietySensor : MonsterSensor
    {
        [Header("=== Satiety Configuration ===")]
        [SerializeField] private float maxSatiety = 60f;
        [SerializeField] private float baseDrainRate = 0.4f;
        [SerializeField] private float hungryThreshold = 40f;
        [SerializeField] private float angryThreshold = 20f;
        [SerializeField] private float killThreshold = 0f;

        [Header("=== State Transitions ===")]
        [SerializeField] private MonsterState hungryState;
        [SerializeField] private MonsterState angryState;
        [SerializeField] private MonsterState killState;

        private float rate;
        private MonsterState lastTriggeredState;

        public float CurrentSatiety { get; private set; }
        public float DrainMultiplier { get; set; } = 1f;

        public override void Initialize(MonsterController owningMonster)
        {
            base.Initialize(owningMonster);
            CurrentSatiety = maxSatiety;
        }

        public override void OnTick(float tickDelta)
        {
            base.OnTick(tickDelta);

            this.rate = baseDrainRate;

            if (controller.Config != null)
            {
                var nightOverride = controller.Config.GetOverrideForNight(controller.CurrentNight);
                this.rate *= nightOverride.aggressionMultiplier;
            }

            var radioSensor = controller.GetSensor<RadioSensor>();
            if (radioSensor != null)
                this.rate *= radioSensor.CurrentAggressionModifier;

            this.rate *= DrainMultiplier;

            float previousSatiety = CurrentSatiety;
            CurrentSatiety = Mathf.Clamp(CurrentSatiety - this.rate * tickDelta, 0f, this.maxSatiety);

            HandleStateTransitions(previousSatiety);
        }

        private void HandleStateTransitions(float previousSatiety)
        {
            // Determine target state based on current satiety
            MonsterState targetState = null;

            if (CurrentSatiety <= killThreshold && killState != null)
                targetState = killState;
            else if (CurrentSatiety <= angryThreshold && previousSatiety > angryThreshold && angryState != null)
                targetState = angryState;
            else if (CurrentSatiety <= hungryThreshold && previousSatiety > hungryThreshold && hungryState != null)
                targetState = hungryState;

            // Only trigger if we have a new target and it's different from last triggered
            if (targetState != null && targetState != lastTriggeredState)
            {
                lastTriggeredState = targetState;
                TriggerTransitionTo(targetState);
            }
        }

        public override void OnStateChanged()
        {
            base.OnStateChanged();
            // Reset so we can trigger the same state again if satiety goes back up and down
            lastTriggeredState = null;
        }

        /// Add (or subtract) satiety.
        public void AddSatiety(float amount)
        {
            CurrentSatiety = Mathf.Clamp(CurrentSatiety + amount, 0f, this.maxSatiety);
        }
    }
}
