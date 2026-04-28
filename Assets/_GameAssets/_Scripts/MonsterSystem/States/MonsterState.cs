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

        // Called by MonsterController before OnStateEnter.
        internal void ProcessAffordancesOnEnter()
        {
            if (this.affordances == null) return;
            for (int i = 0; i < this.affordances.Length; i++)
            {
                if (this.affordances[i]?.TriggerMode == AffordanceTriggerMode.OnStateEnter)
                    this.affordances[i].OnTrigger();
            }
        }

        // Called by MonsterController after OnStateExit.
        internal void ProcessAffordancesOnExit()
        {
            if (this.affordances == null) return;
            for (int i = 0; i < this.affordances.Length; i++)
            {
                var a = this.affordances[i];
                if (a == null) continue;
                if (a.TriggerMode == AffordanceTriggerMode.OnStateEnter)
                    a.OnStop();
                else if (a.TriggerMode == AffordanceTriggerMode.OnStateExit)
                    a.OnTrigger();
            }
        }

        /// <summary>
        /// Triggers all Custom-mode affordances. Use for mid-state affordances not bound to enter/exit.
        /// </summary>
        public void TriggerAffordances()
        {
            if (this.affordances == null) return;
            for (int i = 0; i < this.affordances.Length; i++)
            {
                if (this.affordances[i]?.TriggerMode == AffordanceTriggerMode.Custom)
                    this.affordances[i].OnTrigger();
            }
        }

        /// <summary>
        /// Triggers Custom-mode affordances of the specified type.
        /// Example: TriggerAffordances&lt;AudioAffordance&gt;()
        /// </summary>
        public void TriggerAffordances<T>() where T : StateAffordance
        {
            if (this.affordances == null) return;
            for (int i = 0; i < this.affordances.Length; i++)
            {
                if (this.affordances[i] is T && this.affordances[i].TriggerMode == AffordanceTriggerMode.Custom)
                    this.affordances[i].OnTrigger();
            }
        }

        /// <summary>
        /// Stops all affordances regardless of mode. Use as an imperative override.
        /// </summary>
        public void StopAffordances()
        {
            if (this.affordances == null) return;
            for (int i = 0; i < this.affordances.Length; i++)
                this.affordances[i]?.OnStop();
        }

        /// <summary>
        /// Stops all affordances of the specified type regardless of mode.
        /// Example: StopAffordances&lt;AudioAffordance&gt;()
        /// </summary>
        public void StopAffordances<T>() where T : StateAffordance
        {
            if (this.affordances == null) return;
            for (int i = 0; i < this.affordances.Length; i++)
            {
                if (this.affordances[i] is T)
                    this.affordances[i].OnStop();
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
