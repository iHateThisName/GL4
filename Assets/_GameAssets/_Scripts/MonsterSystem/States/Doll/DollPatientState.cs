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

            if (this.rb != null)
            {
                this.rb.isKinematic = false;

                // Instantly kill all throwing momentum/gravity from the drop
                this.rb.linearVelocity = Vector3.zero;
                this.rb.angularVelocity = Vector3.zero;
            }

            if (this.grabInteractable != null)
            {
                this.grabInteractable.enabled = false;
            }

            // Assuming 'controller' is a protected/public variable from the base MonsterState class
            var petSensor = this.controller.GetSensor<DollSensor>();
            if (petSensor != null)
            {
                petSensor.ResetTimer();
            }

            Debug.Log("Doll is Patient (Slouched). Physics ON. Momentum killed. Grabbing OFF.");
        }

        public override void OnStateExit()
        {
            base.OnStateExit();
        }
    }
}