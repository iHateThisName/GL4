using System;
using System.Collections.Generic;
using UnityEngine;

namespace MonsterSystem {
    /// <summary>
    /// State that triggers affordances on enter and waits for animation events.
    /// Works with AnimationStateChange (StateMachineBehaviour) to detect when the animation
    /// reaches a configured normalized time. Multiple AnimationStateChange SMBs can sit on
    /// the same Animator state — each one routes to a callback in <see cref="animationEvents"/>
    /// by index, where the index matches the SMB's position among AnimationStateChange
    /// behaviours on that state (top in the inspector = 0).
    ///
    /// Subclasses register additional callbacks by overriding <see cref="RegisterAnimationEvents"/>.
    /// Index 0 is always the default end-of-animation handler (<see cref="OnAnimationComplete"/>).
    /// </summary>
    public class AnimatedState : MonsterState
    {
        [Header("Transition")]
        [SerializeField] protected MonsterState nextState;
        [SerializeField] private bool exitOnComplete = true;

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
        /// Ordered callbacks invoked by AnimationStateChange behaviours, by index.
        /// Built once during <see cref="Initialize"/> via <see cref="RegisterAnimationEvents"/>
        /// and then frozen as an array for runtime dispatch.
        /// </summary>
        private Action[] animationEvents;

        // Registration buffer used only during Initialize. Cleared once frozen.
        private List<Action> registrationBuffer;

        public override void Initialize(MonsterController owningController) 
        {
            base.Initialize(owningController);
            this.registrationBuffer = new List<Action>();
            RegisterAnimationEvents();
            this.animationEvents = this.registrationBuffer.ToArray();
            this.registrationBuffer = null;
        }

        /// <summary>
        /// Override to register additional animation event callbacks. Always call base first
        /// so index 0 remains the default end-of-animation handler.
        /// </summary>
        protected virtual void RegisterAnimationEvents() 
        {
            RegisterAnimationEvent(OnAnimationComplete);
        }

        /// <summary>
        /// Called from <see cref="RegisterAnimationEvents"/> to add a callback at the next index.
        /// </summary>
        protected void RegisterAnimationEvent(Action callback) 
        {
            if (this.registrationBuffer == null) {
                Debug.LogError($"[{GetType().Name}] RegisterAnimationEvent called outside of RegisterAnimationEvents()", this);
                return;
            }
            this.registrationBuffer.Add(callback);
        }

        /// <summary>
        /// Called by AnimationStateChange when its configured normalized time is reached
        /// (or when the state exits early, if the SMB has fireOnEarlyExit enabled).
        /// </summary>
        public void InvokeAnimationEvent(int index) 
        {
            if (this.animationEvents == null || index < 0 || index >= this.animationEvents.Length) {
                int have = this.animationEvents?.Length ?? 0;
                Debug.LogWarning($"[{GetType().Name}] No animation event registered at index {index} (have {have})", this);
                return;
            }
            this.animationEvents[index]?.Invoke();
        }

        /// <summary>
        /// Triggers all affordances and begins waiting for animation completion.
        /// </summary>
        public override void OnStateEnter() 
        {
            TriggerAffordances<AnimationAffordance>();
            this.IsAnimating = true;
        }

        /// <summary>
        /// Called by AnimationStateChange when the animation completes.
        /// Calls OnAnimationFinished for subclass cleanup, then transitions to nextState.
        /// </summary>
        public virtual void OnAnimationComplete() 
        {
            this.IsAnimating = false;

            // Transition to the next state if one is assigned
            if (this.nextState != null && exitOnComplete) 
                RequestTransition(this.nextState);
            else 
                Debug.LogWarning($"[{GetType().Name}] Animation finished but no nextState configured!", this);
        }
    }
}
