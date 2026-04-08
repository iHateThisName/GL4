using UnityEngine;

namespace MonsterSystem
{
    /// <summary>
    /// StateMachineBehaviour that fires an indexed animation event on the current
    /// MonsterState (if it is an AnimatedState) when the animation reaches a configured
    /// normalized time. Multiple of these can be attached to the same Animator state —
    /// each one's index is auto-derived from its position in the SMB list (top = 0).
    ///
    /// The default end-of-animation transition is index 0: place a single SMB with
    /// fireAt = 1 and fireOnEarlyExit = true and AnimatedState.OnAnimationComplete will
    /// run on either natural completion or early exit. Add more SMBs above/below for
    /// mid-animation events that route to higher indices.
    /// </summary>
    public class AnimationStateChange : StateMachineBehaviour
    {
        [Tooltip("Normalized time (0-1) at which this event fires. 1 = end of clip.")]
        [Range(0f, 1f)]
        [SerializeField] private float fireAt = 1f;

        [Tooltip("If true, also fire on OnStateExit when the event hasn't fired yet. " +
                 "Leave on for the end-of-animation transition so early exits still notify.")]
        [SerializeField] private bool fireOnEarlyExit = true;

        /// <summary>Editor-only access to the configured normalized fire time (used by the preview).</summary>
        public float FireAt => this.fireAt;

        private MonsterController owningController;
        private int autoIndex;
        private bool hasFired;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateEnter(animator, stateInfo, layerIndex);
            this.hasFired = false;
            this.owningController = animator.transform.root.GetComponentInChildren<MonsterController>();

            // Derive our index from our position among AnimationStateChange SMBs on this state.
            var siblings = animator.GetBehaviours(stateInfo.fullPathHash, layerIndex);
            this.autoIndex = 0;
            int matched = 0;
            for (int i = 0; i < siblings.Length; i++)
            {
                if (siblings[i] is AnimationStateChange other)
                {
                    if (other == this) { this.autoIndex = matched; break; }
                    matched++;
                }
            }
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateUpdate(animator, stateInfo, layerIndex);

            if (this.hasFired) return;
            if (stateInfo.loop) return;
            if (stateInfo.normalizedTime < this.fireAt) return;

            this.hasFired = true;
            FireEvent();
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            base.OnStateExit(animator, stateInfo, layerIndex);

            if (!this.hasFired && this.fireOnEarlyExit)
            {
                this.hasFired = true;
                FireEvent();
            }
        }

        private void FireEvent()
        {
            if (this.owningController == null) return;

            if (this.owningController.CurrentState is AnimatedState animatedState)
            {
                animatedState.InvokeAnimationEvent(this.autoIndex);
            }
        }
    }
}
