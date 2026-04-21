using UnityEngine;

namespace MonsterSystem
{
    public class ResourceSensor : MonsterSensor
    {
        [SerializeField] private float maxResource = 100;
        [SerializeField] private float[] transitionValues;
        [SerializeField] private MonsterState[] statesToTransitionTo;
        [SerializeField] private float baseDepletionValue = -1;
        
        private float resourceValue;
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

            for (int i = 0; i < this.transitionValues.Length; i++)
            {
                if (this.resourceValue <= this.transitionValues[i])
                {
                    if (this.controller.CurrentState == this.statesToTransitionTo[i]) continue;
                    TriggerTransitionTo(this.statesToTransitionTo[i]);
                }
            }

            if (this.resourceValue <= this.minTransitionValue)
                this.thresholdExhausted = true;
        }

        public float Value => this.resourceValue;

        public float ModValue(float incomingChange)
        {
            this.resourceValue += incomingChange;
            if (this.thresholdExhausted && this.resourceValue > this.minTransitionValue)
                this.thresholdExhausted = false;
            return this.resourceValue;
        }
    }
}