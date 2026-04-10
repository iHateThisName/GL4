using UnityEngine;

namespace MonsterSystem
{
    public abstract class MonsterState : MonoBehaviour
    {
        [Header("Transition Blocking")]
        [SerializeField] private bool blocksTransitions;

        private StateAffordance[] affordances;
        protected MonsterController controller;
        
        public virtual void Initialize(MonsterController owningController)
        {
            this.controller = owningController;
            this.affordances = GetComponents<StateAffordance>();

            for (int i = 0; i < this.affordances.Length; i++)
                this.affordances[i].Initialize(owningController);
        }

        /// Called when this state becomes active.
        public virtual void OnStateEnter() { }

        /// Called when leaving this state for another.
        public virtual void OnStateExit() { }

        /// <summary>
        /// Triggers all configured affordances (audio, animation, etc.)
        /// </summary>
        protected void TriggerAffordances()
        {
            if (this.affordances == null) return;
            for (int i = 0; i < this.affordances.Length; i++)
                this.affordances[i]?.Trigger();
        }

        /// <summary>
        /// Triggers only affordances of the specified type.
        /// Example: TriggerAffordances&lt;AudioAffordance&gt;()
        /// </summary>
        protected void TriggerAffordances<T>() where T : StateAffordance
        {
            if (this.affordances == null) return;
            for (int i = 0; i < this.affordances.Length; i++)
            {
                if (this.affordances[i] is T)
                    this.affordances[i].Trigger();
            }
        }

        /// <summary>
        /// Stops all configured affordances.
        /// </summary>
        protected void StopAffordances()
        {
            if (this.affordances == null) return;
            for (int i = 0; i < this.affordances.Length; i++)
                this.affordances[i]?.Stop();
        }

        /// <summary>
        /// Stops only affordances of the specified type.
        /// Example: StopAffordances&lt;AudioAffordance&gt;()
        /// </summary>
        public void StopAffordances<T>() where T : StateAffordance
        {
            if (this.affordances == null) return;
            for (int i = 0; i < this.affordances.Length; i++)
            {
                if (this.affordances[i] is T)
                    this.affordances[i].Stop();
            }
        }

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
        
        protected void KillPlayer()
        {
            string monsterName = this.controller.transform.parent.name;
            monsterName = monsterName.Replace("(Clone)", "").Replace("Prefab", "");

            // Notify the death system that the player was killed by a monster
            DeathSystem.KillPlayer(DeathSystem.DeathEvent.DeathReason.Monster, monsterName);
        }
        
        /// <summary>
        /// If true, prevents sensors and other states from triggering transitions away from this state.
        /// Useful for states like KillPlayer that should not be interrupted.
        /// </summary>
        public bool BlocksTransitions => blocksTransitions;
    }
}
