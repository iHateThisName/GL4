using UnityEngine;

namespace MonsterSystem
{
    /// <summary>
    /// A timed idle state that waits for the timer to finish,
    /// then transitions to the configured next state.
    /// </summary>
    public class IdleState : MonsterStateWithTimer
    {
        [SerializeField] private MonsterState nextState; // State to transition to when the idle timer expires

        /// <summary>
        /// Called when the idle timer completes. Transitions to the next state if one is assigned.
        /// </summary>
        protected override void OnTimerFinished()
        {
            base.OnTimerFinished();

            // Only transition if a next state has been configured in the Inspector
            if (this.nextState != null)
                this.controller.TransitionTo(this.nextState);
        }
    }
}
