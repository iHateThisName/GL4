using System.Threading;
using UnityEngine;

namespace MonsterSystem
{
    public class DollPatientState : MonsterState
    {
        // Pasta :D
        [SerializeField] private TriggerArea pettingArea;
        [SerializeField] private float maxAcceptableVelocity = 2f;

        [SerializeField] private float timeBeforeDeathCall = 1.0f;
        private CancellationTokenSource deathCts;

        public override void OnStateEnter()
        {
            //Reset timer
            var petSensor = this.controller.GetSensor<DollSensor>();
            petSensor.ResetTimer();

            Debug.Log("Doll is Patient (Slouched).");
            // Controller.Animator.SetTrigger("Slouch");
            //this.pettingArea.OnTriggerEntered += PettingDoll;
        }

        public override void OnStateExit()
        {
            //this.pettingArea.OnTriggerEntered -= PettingDoll;
        }

        //    public void PettingDoll(Collider collider)
        //    {
        //        // 1. Get the attached rigidbody (the parent), just like your Axe script!
        //        Rigidbody rb = collider.attachedRigidbody;
        //        if (rb == null) return;

        //        // 2. Cache the velocity magnitude
        //        Vector3 velocityAtPoint = rb.GetPointVelocity(collider.transform.position);

        //        // Check if you are gentle enough.
        //        if (velocityAtPoint.magnitude <= this.maxAcceptableVelocity)
        //        {
        //            Debug.Log(velocityAtPoint.magnitude);
        //            Debug.Log("Doll petted, doll happy");

        //            // Just call the sensor's native pet method instead of doing math
        //            var petSensor = this.controller.GetSensor<DollSensor>();
        //            petSensor.PetDoll();
        //        }
        //        else
        //        {
        //            Debug.Log("Petting too hard!");
        //            Debug.Log(velocityAtPoint.magnitude);
        //            this.deathCts = new CancellationTokenSource();
        //            _ = DeathSequenceAsync(this.deathCts.Token);
        //        }
        //    }

        //    private async Awaitable DeathSequenceAsync(CancellationToken ct)
        //    {
        //        await Awaitable.WaitForSecondsAsync(this.timeBeforeDeathCall, ct);

        //        if (!ct.IsCancellationRequested)
        //            TriggerImmediateKill();
        //    }

        //    private void TriggerImmediateKill()
        //    {
        //        DeathSystem.KillPlayer(DeathSystem.DeathEvent.DeathReason.Monster);
        //    }
        //}
    }
}