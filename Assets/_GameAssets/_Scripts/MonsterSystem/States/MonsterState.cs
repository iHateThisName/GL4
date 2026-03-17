using UnityEngine;

namespace MonsterSystem
{
    public abstract class MonsterState : MonoBehaviour
    {
        [Header("Transition Blocking")]
        [SerializeField] private bool blocksTransitions;

        protected MonsterController controller;

        /// <summary>
        /// If true, prevents sensors and other states from triggering transitions away from this state.
        /// Useful for states like KillPlayer that should not be interrupted.
        /// </summary>
        public bool BlocksTransitions => blocksTransitions;

        /// Called when this state becomes active.
        public virtual void OnStateEnter() { }

        /// Called every tick by MonsterStateManager (NOT every frame).
        public virtual void OnStateTick(float tickDelta) { }

        /// Called when leaving this state for another.
        public virtual void OnStateExit() { }

        /// Helper: request an imperative transition from within a state.
        protected void RequestTransition(MonsterState targetState)
        {
            MonsterStateManager.RequestTransition(controller, targetState);
        }

        /// <summary>
        /// Request a transition with typed context data.
        /// The target state should implement IStateWithContext&lt;T&gt; to receive the context.
        /// </summary>
        protected void RequestTransition<T>(MonsterState targetState, T context)
        {
            MonsterStateManager.RequestTransition(controller, targetState, context);
        }

        public virtual void Initialize(MonsterController owningController)
        {
            this.controller = owningController;
        }
    }
}
