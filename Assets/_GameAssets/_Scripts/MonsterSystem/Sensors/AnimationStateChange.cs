using UnityEngine;

namespace MonsterSystem
{
    /// <summary>
    /// StateMachineBehaviour that notifies the monster state machine when an animation completes.
    /// Attach this to animation states in the Animator to trigger state transitions.
    ///
    /// If the current MonsterState is an AnimatedState, it will call OnAnimationComplete() which
    /// allows the state to perform cleanup before transitioning to its configured next state.
    ///
    /// If a fallback state is configured and the current state is NOT an AnimatedState,
    /// it will transition directly to the fallback state.
    /// </summary>
    public class AnimationStateChange : StateMachineBehaviour
    {
        [Tooltip("Optional fallback state if current state is not an AnimatedState")]
        [SerializeField] private MonsterState fallbackState;

        private MonsterController owningController;
        private bool hasCompleted;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            this.hasCompleted = false;
            //this.owningController = animator.GetComponentInParent<MonsterController>();
            this.owningController = animator.transform.root.GetComponentInChildren<MonsterController>();
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);

            // Check if animation has finished playing (non-looping animations)
            if (this.hasCompleted) return;
            if (stateInfo.loop) return;
            if (stateInfo.normalizedTime < 1f) return;

            this.hasCompleted = true;
            NotifyCompletion();
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);

            // Also notify on exit in case animation transitions early
            if (!this.hasCompleted)
            {
                this.hasCompleted = true;
                NotifyCompletion();
            }
        }

        private void NotifyCompletion()
        {
            if (this.owningController == null) return;

            // If current state is an AnimatedState, let it handle the transition
            if (this.owningController.CurrentState is AnimatedState animatedState)
            {
                animatedState.OnAnimationComplete();
            }
            // Otherwise use the fallback state if configured
            else if (this.fallbackState != null)
            {
                this.owningController.TransitionTo(this.fallbackState);
            }
        }
    }
}