using System.Threading;
using UnityEngine;

namespace MonsterSystem
{
    public class DollAttackState : MonsterState
    {
        [Header("Jumpscare Positioning")]
        [Tooltip("How close the doll's center is to the player's eyes.")]
        [SerializeField] private float faceProximity = 2f;
        [Tooltip("Slight vertical offset if the doll is too high/low relative to its pivot.")]
        [SerializeField] private float verticalOffset = -0.1f;

        [Header("Timing")]
        [Tooltip("Seconds the doll is attached to the face before the final death screen fades in.")]
        [SerializeField] private float timeBeforeDeathCall = 1.0f;

        [SerializeField] private SO_TransformRef playerRef;

        private DollSensor sensor;
        private Transform playerTransform;
        private Transform rootParent;
        private CancellationTokenSource deathCts;

        public override void Initialize(MonsterController controller)
        {
            base.Initialize(controller);
            this.sensor = controller.GetSensor<DollSensor>();
            this.playerTransform = this.playerRef?.Value;
            this.rootParent = controller.transform;
        }

        public override void OnStateEnter()
        {
            this.playerTransform = this.playerRef?.Value;
            Debug.Log("THEDOLLHASYOU.");

            if (this.playerTransform != null)
            {
                AttachToPlayerFace();
            }
            else
            {
                Debug.LogError("DollAttackState: Player Transform not found on DollSensor! Triggering emergency kill.");
                TriggerImmediateKill();
                return;
            }

            this.deathCts = new CancellationTokenSource();
            _ = DeathSequenceAsync(this.deathCts.Token);
        }

        private void AttachToPlayerFace()
        {
            Debug.Log("attach");
            this.rootParent.SetParent(this.playerTransform);
            this.rootParent.localPosition = (Vector3.forward * faceProximity) + (Vector3.up * verticalOffset);
            this.rootParent.localRotation = Quaternion.Euler(0, 180, 0);
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

        public override void OnStateExit()
        {
            if (this.deathCts != null)
            {
                this.deathCts.Cancel();
                this.deathCts.Dispose();
                this.deathCts = null;
            }
        }
    }
}
