using MonsterSystem;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MonsterSystem
{
    public class DollPatientState : MonsterState
    {
        [Header("=== Components ===")]
        [SerializeField] private Rigidbody rb;
        [SerializeField] private XRGrabInteractable grabInteractable;

        public override void OnStateEnter()
        {
            base.OnStateEnter();

            if (rb != null)
            {
                rb.isKinematic = false;

                // Instantly kill all throwing momentum/gravity from the drop
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            if (grabInteractable != null) grabInteractable.enabled = false;

            var petSensor = this.controller.GetSensor<DollSensor>();
            if (petSensor != null) petSensor.ResetTimer();

            Debug.Log("Doll is Patient (Slouched). Physics ON. Momentum killed. Grabbing OFF.");
        }

        public override void OnStateExit()
        {
            base.OnStateExit();
        }
    }
}