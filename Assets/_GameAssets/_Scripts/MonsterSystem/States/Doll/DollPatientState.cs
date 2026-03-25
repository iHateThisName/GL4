using System.Threading;
using UnityEngine;

namespace MonsterSystem
{
    public class DollPatientState : MonsterState
    {
        [SerializeField] private TriggerArea pettingArea;
        [SerializeField] private float maxAcceptableVelocity = 2f;

        [SerializeField] private float timeBeforeDeathCall = 1.0f;
        private CancellationTokenSource deathCts;

        public override void OnStateEnter()
        {
            Debug.Log("Doll is Patient (Slouched).");
            // Controller.Animator.SetTrigger("Slouch");
            this.pettingArea.OnTriggerEntered += PetDoll;
        }

        public override void OnStateExit()
        {
            this.pettingArea.OnTriggerEntered -= PetDoll;
        }

        public void PetDoll(Collider collider)
        {
            // 2. If there is no Rigidbody, stop right here.
            Rigidbody rb = collider.GetComponent<Rigidbody>();
            if (rb == null) return;

            // Cache the velocity magnitude so Unity only calculates it once
            Vector3 velocityAtPoint = rb.GetPointVelocity(collider.transform.position);

            // Check if you are gentle enough.
            if (velocityAtPoint.magnitude <= this.maxAcceptableVelocity)
            {
                Debug.Log(velocityAtPoint.magnitude);
                Debug.Log("Doll petted, doll happy");
                var petSensor = this.controller.GetSensor<DollSensor>();
                petSensor.ReducePatience(-20f);
            }
            else
            {
                Debug.Log("Petting too hard!");
                Debug.Log(velocityAtPoint.magnitude);
                this.deathCts = new CancellationTokenSource();
                _ = DeathSequenceAsync(this.deathCts.Token);
            }
        }

        private async Awaitable DeathSequenceAsync(CancellationToken ct)
        {
            await Awaitable.WaitForSecondsAsync(this.timeBeforeDeathCall, ct);

            if (!ct.IsCancellationRequested)
                TriggerImmediateKill();
        }

        private void TriggerImmediateKill()
        {
            DeathSystem.KillPlayer(DeathSystem.DeathEvent.DeathReason.Monster);
        }
    }
}