using UnityEngine;

namespace MonsterSystem
{
    /// <summary>
    /// StateMachineBehaviour that notifies the monster state machine when an animation completes.
    /// Attach this to animation states in the Animator to trigger state transitions on animation exit.
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

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            var monster = animator.transform.root;
            this.owningController = monster.GetComponent<MonsterController>();
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);
            if (this.owningController == null) return;

            // If current state is an AnimatedState, let it handle the transition
            if (this.owningController.CurrentState is AnimatedState animatedState)
            {
                animatedState.OnAnimationComplete();
            }
            // Otherwise use the fallback state if configured
            else if (fallbackState != null)
            {
                this.owningController.TransitionTo(fallbackState);
            }
        }
    }
}