using UnityEngine;

namespace MonsterSystem {
    /// <summary>
    /// State that sets an animation parameter on enter and waits for completion.
    /// Works with AnimationStateChange (StateMachineBehaviour) to detect when the animation finishes.
    /// Supports Trigger, Bool, Float, Int, or None parameter types.
    /// </summary>
    public class AnimatedState : MonsterState {
        [SerializeField] private EnumAnimationStates AnimationState;

        [Header("Transition")]
        [SerializeField] protected MonsterState nextState; // State to transition to after animation completes

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
        /// Sets the configured animation parameter on enter and begins waiting for completion.
        /// If no parameter is configured, completes immediately.
        /// </summary>
        public override void OnStateEnter() {
            // No animation to play — complete immediately
            if (AnimationTriggers.GetTriggerHash(this.AnimationState) == 0) {
                this.IsAnimating = false;
                this.OnAnimationComplete();
                return;
            }

            this.IsAnimating = true; // Flag
            Animator animator = this.controller.Animator; // Cache the Animator reference for the switch block
            MonsterAnimation.SetTrigger(animator, AnimationTriggers.GetTriggerHash(this.AnimationState));

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
                this.controller.TransitionTo(this.nextState);
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
