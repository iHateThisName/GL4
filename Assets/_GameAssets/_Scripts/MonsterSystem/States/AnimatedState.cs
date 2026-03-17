using UnityEngine;

namespace MonsterSystem
{
    /// <summary>
    /// State that plays an animation and waits for it to complete.
    /// Works with AnimationStateChange (StateMachineBehaviour) to detect animation completion.
    /// </summary>
    public class AnimatedState : MonsterState
    {
        [SerializeField] private string animTrigger;
        [SerializeField] protected MonsterState nextState;

        public override void OnStateEnter()
        {
            MonsterAnimation.SetTrigger(this.controller.Animator, this.animTrigger);
        }

        /// <summary>
        /// Called by AnimationStateChange when the animation completes.
        /// Override this to perform cleanup before transitioning to the next state.
        /// </summary>
        public virtual void OnAnimationComplete()
        {
            OnAnimationFinished();

            if (nextState != null)
                controller.TransitionTo(nextState);
        }

        /// <summary>
        /// Override this to perform work when the animation finishes.
        /// Called before transitioning to the next state.
        /// </summary>
        protected virtual void OnAnimationFinished() { }
    }
}