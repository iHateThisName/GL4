using UnityEngine;

namespace MonsterSystem
{
    public enum AffordanceTriggerMode
    {
        OnStateEnter,
        OnStateExit,
        Custom
    }

    /// <summary>
    /// Base class for state affordances - reusable behaviors that states can trigger.
    /// Affordances decouple what happens (animation, audio) from state logic.
    /// </summary>
    public abstract class StateAffordance : MonoBehaviour
    {
        [SerializeField] private AffordanceTriggerMode triggerMode = AffordanceTriggerMode.Custom;

        protected MonsterController controller;

        public AffordanceTriggerMode TriggerMode => triggerMode;

        /// <summary>
        /// Called by MonsterState to provide access to the controller.
        /// </summary>
        public virtual void Initialize(MonsterController owningController)
        {
            this.controller = owningController;
        }

        /// <summary>
        /// Activate this affordance (play audio, trigger animation, etc.)
        /// </summary>
        public abstract void OnTrigger();

        /// <summary>
        /// Stop/cancel this affordance if applicable.
        /// </summary>
        public virtual void OnStop() { }
    }
}
