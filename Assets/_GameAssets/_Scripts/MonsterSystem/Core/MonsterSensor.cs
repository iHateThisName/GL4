using UnityEngine;

namespace MonsterSystem
{
    public abstract class MonsterSensor : MonoBehaviour
    {
        [SerializeField] private MonsterState stateToTransitionTo;

        protected MonsterController controller;
        private bool hasTriggeredTransition;

        /// <summary>
        /// Time elapsed since the last tick. Use this for time-based calculations.
        /// </summary>
        protected float TickDelta { get; private set; }

        /// <summary>
        /// Called by MonsterController during the tick cycle.
        /// Override to implement sensor logic. Always call base.OnTick(tickDelta) first.
        /// </summary>
        public virtual void OnTick(float tickDelta)
        {
            this.TickDelta = tickDelta;
        }

        public virtual void Initialize(MonsterController owningMonster)
        {
            this.controller = owningMonster;
        }

        /// <summary>
        /// Called when the monster transitions to a new state.
        /// Resets the transition flag so the sensor can trigger again.
        /// </summary>
        public virtual void OnStateChanged()
        {
            this.hasTriggeredTransition = false;
        }

        /// <summary>
        /// Triggers transition to the configured state.
        /// Respects BlocksTransitions and only fires once per state.
        /// </summary>
        protected void TriggerStateTransition()
        {
            if (hasTriggeredTransition || controller.IsBlockingTransitions) return;
            hasTriggeredTransition = true;
            controller.TransitionTo(stateToTransitionTo);
        }

        /// <summary>
        /// Triggers transition to the configured state with typed context data.
        /// Respects BlocksTransitions and only fires once per state.
        /// </summary>
        protected void TriggerStateTransition<T>(T context)
        {
            if (hasTriggeredTransition || controller.IsBlockingTransitions) return;
            hasTriggeredTransition = true;
            controller.TransitionTo(stateToTransitionTo, context);
        }

        /// <summary>
        /// Triggers transition to a specific state.
        /// Respects BlocksTransitions and only fires once per state.
        /// </summary>
        protected void TriggerTransitionTo(MonsterState state)
        {
            if (hasTriggeredTransition || controller.IsBlockingTransitions) return;
            hasTriggeredTransition = true;
            controller.TransitionTo(state);
        }

        /// <summary>
        /// Triggers transition to a specific state with typed context data.
        /// Respects BlocksTransitions and only fires once per state.
        /// </summary>
        protected void TriggerTransitionTo<T>(MonsterState state, T context)
        {
            if (hasTriggeredTransition || controller.IsBlockingTransitions) return;
            hasTriggeredTransition = true;
            controller.TransitionTo(state, context);
        }
    }
}
