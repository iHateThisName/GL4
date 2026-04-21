using UnityEngine;

namespace MonsterSystem
{
    public abstract class MonsterSensor : MonoBehaviour
    {
        [SerializeField] private MonsterState stateToTransitionTo;

        protected MonsterController controller;
        private bool hasTriggeredTransition;
        private bool hasStarted;

        /// <summary>
        /// Time elapsed since the last tick. Use this for time-based calculations.
        /// </summary>
        protected float TickDelta { get; private set; }

        #region UnityHooks
        protected virtual void Start()
        {
            hasStarted = true;
            Subscribe();
        }

        protected virtual void OnEnable()
        {
            // Only subscribe in OnEnable if Start has already run (re-enabling)
            if (hasStarted)
                Subscribe();
        }

        protected virtual void OnDisable() => Unsubscribe();
        #endregion
        
        public virtual void Initialize(MonsterController owningMonster)
        {
            this.controller = owningMonster;
        }

        /// <summary>
        /// Called by MonsterController during the tick cycle.
        /// Override to implement sensor logic. Always call base.OnTick(tickDelta) first.
        /// </summary>
        public virtual void OnTick(float tickDelta)
        {
            this.TickDelta = tickDelta;
        }

        /// <summary>
        /// Override to subscribe to events. Called in Start and on re-enable.
        /// </summary>
        protected virtual void Subscribe() { }

        /// <summary>
        /// Override to unsubscribe from events. Called in OnDisable.
        /// </summary>
        protected virtual void Unsubscribe() { }

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
            if (this.hasTriggeredTransition || this.controller.IsBlockingTransitions) return;
            this.hasTriggeredTransition = true;
            this.controller.TransitionTo(this.stateToTransitionTo);
        }

        /// <summary>
        /// Triggers transition to the configured state with typed context data.
        /// Respects BlocksTransitions and only fires once per state.
        /// </summary>
        protected void TriggerStateTransition<T>(T context)
        {
            if (this.hasTriggeredTransition || this.controller.IsBlockingTransitions) return;
            this.hasTriggeredTransition = true;
            this.controller.TransitionTo(this.stateToTransitionTo, context);
        }

        /// <summary>
        /// Triggers transition to a specific state.
        /// Respects BlocksTransitions and only fires once per state.
        /// </summary>
        protected void TriggerTransitionTo(MonsterState state)
        {
            if (this.hasTriggeredTransition || this.controller.IsBlockingTransitions) return;
            this.hasTriggeredTransition = true;
            this.controller.TransitionTo(state);
        }

        /// <summary>
        /// Triggers transition to a specific state with typed context data.
        /// Respects BlocksTransitions and only fires once per state.
        /// </summary>
        protected void TriggerTransitionTo<T>(MonsterState state, T context)
        {
            if (this.hasTriggeredTransition || this.controller.IsBlockingTransitions) return;
            this.hasTriggeredTransition = true;
            this.controller.TransitionTo(state, context);
        }
    }
}
