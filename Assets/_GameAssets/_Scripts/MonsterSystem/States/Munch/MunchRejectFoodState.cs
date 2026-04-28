using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MonsterSystem
{
    /// <summary>
    /// Plays a reject animation, throws the food away, reduces satiety, and plays a reject sound.
    /// Used when the monster refuses food (e.g., food velocity too high).
    /// </summary>
    public class MunchRejectFoodState : AnimatedState, IStateWithContext<Rigidbody>
    {
        [SerializeField] private float satietyLoss;                                      // Amount of satiety subtracted when food is rejected
        [SerializeField] private float throwForce = 5f;                                  // Impulse magnitude applied to throw the food away
        [SerializeField] private Vector3 throwDirection = new Vector3(0f, 1f, 1f);       // Local-space direction the food is thrown toward

        private ResourceSensor satietySensor; // Reference to the monster's satiety sensor component
        private Rigidbody foodRb;            // Rigidbody of the rejected food item

        /// <summary>
        /// Initializes the state by caching a reference to the monster's satiety sensor.
        /// </summary>
        public override void Initialize(MonsterController owningController)
        {
            base.Initialize(owningController);
            this.satietySensor = owningController.GetSensor<ResourceSensor>();
        }

        /// <summary>
        /// Receives the food Rigidbody context passed during state transition.
        /// </summary>
        public void ReceiveContext(Rigidbody context)
        {
            this.foodRb = context;
        }

        /// <summary>
        /// On entering the reject state: plays the reject sound, force-releases and throws the food,
        /// and reduces the monster's satiety.
        /// </summary>
        public override void OnStateEnter()
        {
            base.OnStateEnter();

            // Release the food from any XR grab and throw it away from the monster
            if (this.foodRb != null)
            {
                var grab = this.foodRb.GetComponent<XRGrabInteractable>();
                ForceRelease(grab);

                // Convert throw direction from local space to world space and apply impulse
                Vector3 worldThrowDir = this.controller.transform.TransformDirection(this.throwDirection.normalized);
                this.foodRb.AddForce(worldThrowDir * this.throwForce, ForceMode.Impulse);
            }

            // Reduce satiety by the configured loss amount
            if (this.satietySensor != null)
                this.satietySensor.ModValue(-this.satietyLoss);
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
