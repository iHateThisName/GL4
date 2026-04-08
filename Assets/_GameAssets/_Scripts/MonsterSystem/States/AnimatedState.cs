using UnityEngine;

namespace MonsterSystem {
    /// <summary>
    /// State that triggers affordances on enter and waits for animation completion.
    /// Works with AnimationStateChange (StateMachineBehaviour) to detect when the animation finishes.
    /// Use AnimationAffordance to configure which animation to trigger.
    /// </summary>
    public class AnimatedState : MonsterState {
        [field: SerializeField] public EnumAnimationStates AnimationState { private set; get; }

        [Header("Transition")]
        [SerializeField] protected MonsterState nextState;
        [SerializeField] private bool exitOnComplete = true;

        [Tooltip("When true, wait for AnimationStateChange callback even if animationState is None (use with AnimationAffordance)")]
        [SerializeField] private bool waitForAffordanceAnimation;

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
            // Always trigger affordances first (they may set animator parameters)
            TriggerAffordances<AnimationAffordance>();

            // If using enum-based animation state, set the trigger
            if (this.AnimationState != EnumAnimationStates.None)
            {
                this.IsAnimating = true;
                MonsterAnimation.SetTrigger(this.controller.Animator, AnimationTriggers.GetTriggerHash(this.AnimationState));
            }
            // If using affordance-based animation, wait for callback
            else if (this.waitForAffordanceAnimation)
            {
                this.IsAnimating = true;
            }
            // No animation configured — complete immediately
            else
            {
                this.IsAnimating = false;
                this.OnAnimationComplete();
            }
        }

        /// <summary>
        /// Called by AnimationStateChange when the animation completes.
        /// Calls OnAnimationFinished for subclass cleanup, then transitions to nextState.
        /// </summary>
        public virtual void OnAnimationComplete() {
            this.IsAnimating = false;

            // Transition to the next state if one is assigned
            if (this.nextState != null && exitOnComplete) {
                RequestTransition(this.nextState);
            } else {
                Debug.LogWarning($"[{GetType().Name}] Animation finished but no nextState configured!", this);
            }
        }
    }
}
