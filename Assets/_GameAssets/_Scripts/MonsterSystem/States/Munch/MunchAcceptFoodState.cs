using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MonsterSystem
{
    /// <summary>
    /// Plays an accept/eat animation. Two AnimationStateChange SMBs sit on the animation node:
    ///   - Index 0 (fireAt = 1, fireOnEarlyExit = true) → OnAnimationComplete → state transition.
    ///   - Index 1 (fireAt ≈ bite frame, fireOnEarlyExit = false) → HandleEatMoment → consume food.
    /// </summary>
    public class MunchAcceptFoodState : AnimatedState, IStateWithContext<Rigidbody>
    {
        [SerializeField] private float satietyGain = 25f; // Amount of satiety added when food is accepted

        private SatietySensor satietySensor; // Reference to the monster's satiety sensor component
        private Rigidbody foodRb;            // Rigidbody of the food item being consumed
        private GameObject foodObject;       // Cached game object of the food for destruction after eating

        /// <summary>
        /// Initializes the state by caching a reference to the monster's satiety sensor.
        /// </summary>
        public override void Initialize(MonsterController owningController)
        {
            base.Initialize(owningController);
            this.satietySensor = owningController.GetSensor<SatietySensor>();
        }

        protected override void RegisterAnimationEvents()
        {
            base.RegisterAnimationEvents();          // index 0: OnAnimationComplete (transition)
            RegisterAnimationEvent(HandleEatMoment); // index 1: mid-animation eat moment
        }

        /// <summary>
        /// Receives the food Rigidbody context passed during state transition.
        /// </summary>
        public void ReceiveContext(Rigidbody context)
        {
            this.foodRb = context;
            this.foodObject = context != null ? context.gameObject : null;
        }

        /// <summary>
        /// Mid-animation: release the food from any XR grab, add satiety, play the eat SFX,
        /// and destroy the food object.
        /// </summary>
        private void HandleEatMoment()
        {
            if (this.foodRb != null)
            {
                var grab = this.foodRb.GetComponent<XRGrabInteractable>();
                ForceRelease(grab);
            }

            this.satietySensor.AddSatiety(this.satietyGain);
            TriggerAffordances<AudioAffordance>();

            if (this.foodObject != null) Destroy(this.foodObject);
        }

        /// <summary>
        /// Forces an XR grab interactable to be released from its current selector.
        /// </summary>
        private void ForceRelease(XRGrabInteractable interactable)
        {
            if (interactable != null && interactable.isSelected)
            {
                // Exit the selection between the first selecting interactor and this interactable
                interactable.interactionManager.SelectExit(
                    interactable.firstInteractorSelecting,
                    interactable
                );
            }
        }
    }
}
