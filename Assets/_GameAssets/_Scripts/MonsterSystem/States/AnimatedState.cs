using UnityEngine;

namespace MonsterSystem
{
    /// <summary>
    /// State that sets an animation parameter on enter and waits for completion.
    /// Works with AnimationStateChange (StateMachineBehaviour) to detect when the animation finishes.
    /// Supports Trigger, Bool, Float, Int, or None parameter types.
    /// </summary>
    public class AnimatedState : MonsterState
    {
        // Defines which Animator parameter type this state will set on enter
        public enum AnimParamType { None, Trigger, Bool, Float, Int }

        [Header("Animation")]
        [SerializeField] private AnimParamType paramType = AnimParamType.Trigger; // The type of animation parameter to set
        [SerializeField] private string paramName; // Name of the Animator parameter to set
        [SerializeField] private bool boolValue; // Value used when paramType is Bool
        [SerializeField] private float floatValue; // Value used when paramType is Float
        [SerializeField] private int intValue; // Value used when paramType is Int

        [Header("Transition")]
        [SerializeField] protected MonsterState nextState; // State to transition to after animation completes

        /// <summary>
        /// True from OnStateEnter until OnAnimationComplete is called by AnimationStateChange.
        /// </summary>
        public bool IsAnimating { get; private set; }

        /// <summary>
        /// Normalized animation progress (0-1) of the current Animator state on layer 0.
        /// </summary>
        public float AnimationProgress
        {
            get
            {
                // Return zero if no Animator is available
                if (this.controller.Animator == null) return 0f;
                return Mathf.Clamp01(this.controller.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
            }
        }

        /// <summary>
        /// Sets the configured animation parameter on enter and begins waiting for completion.
        /// If no parameter is configured, completes immediately.
        /// </summary>
        public override void OnStateEnter()
        {
            // No animation to play — complete immediately
            if (this.paramType == AnimParamType.None || string.IsNullOrEmpty(this.paramName))
            {
                this.IsAnimating = false;
                this.OnAnimationComplete();
                return;
            }

            this.IsAnimating = true;

            // Cache the Animator reference for the switch block
            var animator = this.controller.Animator;

            // Set the appropriate Animator parameter based on the configured type
            switch (this.paramType)
            {
                case AnimParamType.Trigger:
                    MonsterAnimation.SetTrigger(animator, this.paramName);
                    break;
                case AnimParamType.Bool:
                    MonsterAnimation.SetBool(animator, this.paramName, this.boolValue);
                    break;
                case AnimParamType.Float:
                    MonsterAnimation.SetFloat(animator, this.paramName, this.floatValue);
                    break;
                case AnimParamType.Int:
                    MonsterAnimation.SetInt(animator, this.paramName, this.intValue);
                    break;
            }
        }

        /// <summary>
        /// Called by AnimationStateChange when the animation completes.
        /// Calls OnAnimationFinished for subclass cleanup, then transitions to nextState.
        /// </summary>
        public virtual void OnAnimationComplete()
        {
            this.IsAnimating = false;
            this.OnAnimationFinished();

            // Transition to the next state if one is assigned
            if (this.nextState != null)
            {
                this.controller.TransitionTo(this.nextState);
            }
            else
            {
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
