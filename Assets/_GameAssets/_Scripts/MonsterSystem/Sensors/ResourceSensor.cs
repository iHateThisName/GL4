using UnityEngine;

namespace MonsterSystem
{
    public class ResourceSensor : MonsterSensor
    {
        [SerializeField] private float maxResource = 100;
        [SerializeField] private float[] transitionValues;
        [SerializeField] private MonsterState[] statesToTransitionTo;
        [SerializeField] private float baseDepletionValue = -1;
        [Gaskellgames.ReadOnly]
        [SerializeField] private float resourceValue;
        
        private float minTransitionValue;
        private bool deplete;
        private bool thresholdExhausted;

        public override void Initialize(MonsterController owningMonster)
        {
            base.Initialize(owningMonster);
            this.resourceValue = this.maxResource;
            this.deplete = !this.baseDepletionValue.Equals(-1);
            this.thresholdExhausted = false;

            this.minTransitionValue = float.MaxValue;
            for (int i = 0; i < this.transitionValues.Length; i++)
                if (this.transitionValues[i] < this.minTransitionValue)
                    this.minTransitionValue = this.transitionValues[i];
        }

        public override void OnTick(float tickDelta)
        {
            base.OnTick(tickDelta);

            if (this.deplete) ModValue(-this.baseDepletionValue * tickDelta);

            if (this.thresholdExhausted) return;

            // Find the most severe (lowest-value) threshold that has been crossed.
            // Checking all exceeded thresholds and firing them all caused oscillation:
            // transitioning to Angry reset hasTriggeredTransition mid-loop, letting the
            // Hungry threshold (also exceeded) fire again immediately every tick.
            MonsterState targetState = null;
            float mostSevereThreshold = float.MaxValue;

            for (int i = 0; i < this.transitionValues.Length; i++)
            {
                if (this.resourceValue <= this.transitionValues[i] && this.transitionValues[i] < mostSevereThreshold)
                {
                    mostSevereThreshold = this.transitionValues[i];
                    targetState = this.statesToTransitionTo[i];
                }
            }

            if (targetState != null && this.controller.CurrentState != targetState)
                TriggerTransitionTo(targetState);

            if (this.resourceValue <= this.minTransitionValue)
                this.thresholdExhausted = true;
        }

        public float Value => this.resourceValue;

        public float ModValue(float incomingChange, bool isFeeding = false)
        {
            this.resourceValue += incomingChange;
            if (this.thresholdExhausted && this.resourceValue > this.minTransitionValue)
                this.thresholdExhausted = false;

            if (isFeeding && this.resourceValue > this.transitionValues[0]) {
                TriggerStateTransition();
            }
            return this.resourceValue;
        }
    }
}