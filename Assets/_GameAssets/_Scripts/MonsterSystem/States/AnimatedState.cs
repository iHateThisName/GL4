using UnityEngine;

namespace MonsterSystem {
    /// <summary>
    /// State that triggers affordances on enter and waits for animation completion.
    /// Works with AnimationStateChange (StateMachineBehaviour) to detect when the animation finishes.
    /// Use AnimationAffordance to configure which animation to trigger.
    /// </summary>
    public class AnimatedState : MonsterState {
        [SerializeField] private EnumAnimationStates animationState;

        [Header("Transition")]
        [SerializeField] protected MonsterState nextState;

        /// <summary>
        /// True from OnStateEnter until OnAnimationComplete is called by AnimationStateChange.
        /// </summary>
        public bool IsAnimating { get; private set; }

        /// <summary>
        /// Normalized animation progress (0-1) of the current Animator state on layer 0.
        /// </summary>
        public float AnimationProgress {
            get {
                // Return zero if no Animator is available
                if (this.controller.Animator == null) return 0f;
                return Mathf.Clamp01(this.controller.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
            }
        }

        /// <summary>
        /// Triggers all affordances and begins waiting for animation completion.
        /// </summary>
        public override void OnStateEnter() {
            // No animation to play — complete immediately
            if (AnimationTriggers.GetTriggerHash(this.animationState) == 0) {
                this.IsAnimating = false;
                this.OnAnimationComplete();
                return;
            }
            
            TriggerAffordances<AnimationAffordance>();
            this.IsAnimating = true; // Flag
            if (this.animationState != EnumAnimationStates.None)
            {
                MonsterAnimation.SetTrigger(this.controller.Animator, AnimationTriggers.GetTriggerHash(this.animationState));
            }
        }

        /// <summary>
        /// Called by AnimationStateChange when the animation completes.
        /// Calls OnAnimationFinished for subclass cleanup, then transitions to nextState.
        /// </summary>
        public virtual void OnAnimationComplete() {
            this.IsAnimating = false;
            this.OnAnimationFinished();

            // Transition to the next state if one is assigned
            if (this.nextState != null) {
                RequestTransition(this.nextState);
            } else {
                Debug.LogWarning($"[{GetType().Name}] Animation finished but no nextState configured!", this);
            }
        }

        /// <summary>
        /// Override to perform work when the animation finishes.
        /// Called before transitioning to the next state.
        /// </summary>
        protected virtual void OnAnimationFinished() { }
    }
}
