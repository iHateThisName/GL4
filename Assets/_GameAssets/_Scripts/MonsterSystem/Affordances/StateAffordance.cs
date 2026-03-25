using UnityEngine;

namespace MonsterSystem
{
    /// <summary>
    /// Base class for state affordances - reusable behaviors that states can trigger.
    /// Affordances decouple what happens (animation, audio) from state logic.
    /// </summary>
    public abstract class StateAffordance : MonoBehaviour
    {
        protected MonsterController controller;

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
        public abstract void Trigger();

        /// <summary>
        /// Stop/cancel this affordance if applicable.
        /// </summary>
        public virtual void Stop() { }
    }
}
