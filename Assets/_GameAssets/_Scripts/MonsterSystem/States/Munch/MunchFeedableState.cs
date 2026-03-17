using UnityEngine;

namespace MonsterSystem
{
    /// Base for Hungry/Angry states. Plays animation on enter, handles food detection via TriggerArea.
    public class MunchFeedableState : AnimatedState
    {
        [Header("Audio")]
        [SerializeField] private AudioClip loopingSound;

        [Header("Feed Zone")]
        [SerializeField] private TriggerArea feedZone;
        [SerializeField] private float maxAcceptableVelocity = 2f;

        [Header("Food Transitions")]
        [SerializeField] private MonsterState rejectState;
        [SerializeField] private MonsterState acceptState;

        public override void OnStateEnter()
        {
            base.OnStateEnter();

            if (feedZone != null)
                feedZone.OnTriggerEntered += HandleFeedTrigger;

            if (loopingSound != null)
                MonsterAudio.Play(controller.Audio, loopingSound, loop: true);
        }

        public override void OnStateExit()
        {
            if (feedZone != null)
                feedZone.OnTriggerEntered -= HandleFeedTrigger;

            MonsterAudio.Stop(controller.Audio);
        }

        private void HandleFeedTrigger(Collider other)
        {
            if (this.controller == null || !other.CompareTag("Food")) return;

            Rigidbody foodRb = other.attachedRigidbody;
            if (foodRb == null) return;

            if (foodRb.linearVelocity.magnitude <= maxAcceptableVelocity)
                this.controller.TransitionTo(acceptState, foodRb);
            else
                this.controller.TransitionTo(rejectState, foodRb);
        }
    }
}
