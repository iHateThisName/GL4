using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MonsterSystem
{
    public class MunchAcceptFoodState : AnimatedState, IStateWithContext<Rigidbody>
    {
        [SerializeField] private float satiatyGain;

        private SatietySensor satietySensor;
        private Rigidbody foodRb;
        private GameObject foodObject;

        public override void Initialize(MonsterController owningController)
        {
            base.Initialize(owningController);
            this.satietySensor = owningController.GetSensor<SatietySensor>();
        }

        public void ReceiveContext(Rigidbody context)
        {
            this.foodRb = context;
            this.foodObject = context != null ? context.gameObject : null;
        }

        protected override void OnAnimationFinished()
        {
            base.OnAnimationFinished();

            if (foodRb != null)
            {
                var grab = foodRb.GetComponent<XRGrabInteractable>();
                ForceRelease(grab);
            }

            this.satietySensor.AddSatiety(satiatyGain);

            var config = this.controller.GetConfig<MunchConfig>();
            if (config != null)
                MonsterAudio.PlayOneShot(this.controller.Audio, config.eatSound);

            if (foodObject != null)
                Destroy(foodObject);
        }

        private void ForceRelease(XRGrabInteractable interactable)
        {
            if (interactable != null && interactable.isSelected)
            {
                interactable.interactionManager.SelectExit(
                    interactable.firstInteractorSelecting,
                    interactable
                );
            }
        }
    }
}