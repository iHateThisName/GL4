using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MonsterSystem
{
    /// <summary>
    /// Plays an accept/eat animation, then on completion: consumes food, adds satiety, transitions.
    /// AnimationStateChange on the animation node calls OnAnimationComplete when the animation finishes.
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

        /// <summary>
        /// Receives the food Rigidbody context passed during state transition.
        /// </summary>
        public void ReceiveContext(Rigidbody context)
        {
            this.foodRb = context;
            this.foodObject = context != null ? context.gameObject : null;
        }

        /// <summary>
        /// Called when the accept/eat animation finishes. Releases the food from any grab,
        /// adds satiety, plays the eat sound, and destroys the food object.
        /// </summary>
        protected override void OnAnimationFinished()
        {
            base.OnAnimationFinished();

            // Force-release the food from any XR grab interaction before consuming it
            if (this.foodRb != null)
            {
                var grab = this.foodRb.GetComponent<XRGrabInteractable>();
                ForceRelease(grab);
            }

            // Increase the monster's satiety by the configured gain amount
            this.satietySensor.AddSatiety(this.satietyGain);

            // Play the eat sound effect if a MunchConfig and AudioSource are available
            var config = this.controller.GetConfig<MunchConfig>();
            if (config != null && this.controller.Audio != null)
                MonsterAudio.PlayOneShot(this.controller.Audio, config.eatSound);

            // Destroy the food game object now that it has been consumed
            if (this.foodObject != null)
                Destroy(this.foodObject);
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
