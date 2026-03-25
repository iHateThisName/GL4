using UnityEngine;

namespace MonsterSystem
{
    /// <summary>
    /// Base state for Hungry/Angry feeding phases. Plays an animation on enter,
    /// listens for food entering the feed zone via TriggerArea, and transitions
    /// to accept or reject states based on the food's velocity.
    /// </summary>
    public class MunchFeedableState : AnimatedState
    {
        [Header("Feed Zone")]
        [SerializeField] private TriggerArea feedZone;                  // Trigger area where food must enter to be detected
        [SerializeField] private float maxAcceptableVelocity = 2f;      // Maximum food speed to be accepted (above this, food is rejected)

        [Header("Food Transitions")]
        [SerializeField] private MonsterState rejectState; // State to transition to when food is thrown too fast
        [SerializeField] private MonsterState acceptState; // State to transition to when food is gently placed

        /// <summary>
        /// Subscribes to the feed zone trigger event and starts looping audio on state entry.
        /// </summary>
        public override void OnStateEnter()
        {
            base.OnStateEnter();

            // Subscribe to the feed zone trigger so we are notified when food enters
            if (this.feedZone != null)
                this.feedZone.OnTriggerEntered += HandleFeedTrigger;
            
            TriggerAffordances<AnimationAffordance>();

            // Start playing the looping sound for this feedable state
           TriggerAffordances<AudioAffordance>();
        }

        /// <summary>
        /// Unsubscribes from the feed zone trigger event and stops audio on state exit.
        /// </summary>
        public override void OnStateExit()
        {
            // Unsubscribe from the feed zone trigger to prevent stale callbacks
            if (this.feedZone != null)
                this.feedZone.OnTriggerEntered -= HandleFeedTrigger;

            // Stop any looping audio that was started on enter
            StopAffordances<AudioAffordance>();
        }

        /// <summary>
        /// Handles a collider entering the feed zone. Checks if it is tagged as food,
        /// then transitions to accept or reject based on the food's velocity.
        /// </summary>
        private void HandleFeedTrigger(Collider other)
        {
            // Ignore non-food objects and guard against missing controller
            if (this.controller == null || !other.CompareTag("Food")) return;

            // Retrieve the food's rigidbody; bail out if none is attached
            Rigidbody foodRb = other.attachedRigidbody;
            if (foodRb == null) return;

            // Accept food if it is moving slowly enough; otherwise reject it
            if (foodRb.linearVelocity.magnitude <= this.maxAcceptableVelocity)
                this.controller.TransitionTo(this.acceptState, foodRb);
            else
                this.controller.TransitionTo(this.rejectState, foodRb);
        }
    }
}
