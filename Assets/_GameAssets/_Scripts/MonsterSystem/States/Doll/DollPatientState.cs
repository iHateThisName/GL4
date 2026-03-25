using UnityEngine;

namespace MonsterSystem
{
    public class DollPatientState : MonsterState
    {
        [SerializeField] private TriggerArea pettingArea;
        [SerializeField] private float maxAcceptableVelocity = 2f;

        public override void OnStateEnter()
        {
            Debug.Log("Doll is Patient (Slouched).");
            // Controller.Animator.SetTrigger("Slouch");

            this.pettingArea.OnTriggerEntered += PetDoll;

        }

        public void PetDoll(Collider collider)
        {
            Rigidbody rb = collider.GetComponent<Rigidbody>();
            if (rb == null) return;
            if (rb.linearVelocity.magnitude > this.maxAcceptableVelocity)
            {
                var petSensor = this.controller.GetSensor<DollSensor>();
                petSensor.ReducePatience(-float.MaxValue);
            }
        }
    }
}