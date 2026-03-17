using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MonsterSystem
{
    public class MunchRejectFoodState : AnimatedState, IStateWithContext<Rigidbody>
    {
        [SerializeField] private float satietyLoss;
        [SerializeField] private float throwForce = 5f;
        [SerializeField] private Vector3 throwDirection = new Vector3(0f, 1f, 1f);

        private SatietySensor satietySensor;
        private Rigidbody foodRb;

        public override void Initialize(MonsterController owningController)
        {
            base.Initialize(owningController);
            this.satietySensor = owningController.GetSensor<SatietySensor>();
        }

        public void ReceiveContext(Rigidbody context)
        {
            this.foodRb = context;
        }

        public override void OnStateEnter()
        {
            base.OnStateEnter();

            var config = this.controller.GetConfig<MunchConfig>();
            if (config != null)
                MonsterAudio.PlayOneShot(this.controller.Audio, config.rejectSound);

            if (foodRb != null)
            {
                var grab = foodRb.GetComponent<XRGrabInteractable>();
                ForceRelease(grab);

                foodRb.AddForce(throwDirection.normalized * this.throwForce, ForceMode.Impulse);
            }

            this.satietySensor.AddSatiety(-this.satietyLoss);
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