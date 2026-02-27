using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MonsterSystem
{
    /// Abstract base for Hungry/Angry states. Handles food detection via TriggerArea
    /// and inline accept/reject logic.
    public abstract class MunchFeedableState : MonsterState
    {
        [Header("Feed Zone")]
        [SerializeField] private TriggerArea feedZone;
        [SerializeField] private float maxAcceptableVelocity = 2f;
        [SerializeField] private string rejectTrigger = "Reject";
        [SerializeField] private float throwForce = 5f;
        [SerializeField] private Vector3 throwDirection = new Vector3(0f, 1f, 1f);

        [Header("Transitions")]
        [SerializeField] private MonsterState fedState;

        private MonsterController cachedController;

        public override void OnStateEnter(MonsterController controller)
        {
            cachedController = controller;

            if (feedZone != null)
                feedZone.OnTriggerEntered += HandleFeedTrigger;

            OnFeedableEnter(controller);
        }

        public override void OnStateExit(MonsterController controller)
        {
            if (feedZone != null)
                feedZone.OnTriggerEntered -= HandleFeedTrigger;

            OnFeedableExit(controller);

            cachedController = null;
        }

        protected abstract void OnFeedableEnter(MonsterController controller);

        protected virtual void OnFeedableExit(MonsterController controller) { }

        private void HandleFeedTrigger(Collider other)
        {
            if (cachedController == null) return;

            Rigidbody rb = other.attachedRigidbody;
            if (rb == null) return;

            Food food = rb.GetComponent<Food>();
            if (food != null && rb.linearVelocity.magnitude <= maxAcceptableVelocity)
            {
                Accept(food, rb);
            }
            else
            {
                Reject(rb);
            }
        }

        private void Accept(Food food, Rigidbody rb)
        {
            var grab = rb.GetComponent<XRGrabInteractable>();
            ForceRelease(grab);

            var sensor = cachedController.GetSensor<SatietySensor>();
            if (sensor != null)
                sensor.AddSatiety(food.GetFoodValue(), cachedController);

            var config = cachedController.GetConfig<MunchConfig>();
            if (config != null)
                MonsterAudio.PlayOneShot(cachedController.Audio, config.eatSound);

            Destroy(food.gameObject, 0.2f);

            RequestTransition(cachedController, fedState);
        }

        private void Reject(Rigidbody rb)
        {
            MonsterAnimation.SetTrigger(cachedController.Animator, rejectTrigger);

            var config = cachedController.GetConfig<MunchConfig>();
            if (config != null)
                MonsterAudio.PlayOneShot(cachedController.Audio, config.rejectSound);

            var grab = rb.GetComponent<XRGrabInteractable>();
            ForceRelease(grab);

            rb.AddForce(throwDirection.normalized * throwForce, ForceMode.Impulse);

            var sensor = cachedController.GetSensor<SatietySensor>();
            if (sensor != null && config != null)
                sensor.AddSatiety(-config.rejectPenalty, cachedController);
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
