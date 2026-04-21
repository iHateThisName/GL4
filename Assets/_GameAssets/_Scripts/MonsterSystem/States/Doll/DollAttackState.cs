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
            this.rootParent = controller.transform.parent;
        }

        public override void OnStateEnter()
        {
            this.TriggerAffordances<AudioAffordance>();
            this.playerTransform = this.playerRef?.Value;
            Debug.Log("THEDOLLHASYOU.");

            if (this.playerTransform != null)
            {
                this.AttachToPlayerFace();
            }
            else
            {
                Debug.LogError("DollAttackState: Player Transform not found on DollSensor! Triggering emergency kill.");
                this.TriggerImmediateKill();
                return;
            }

            this.deathCts = new CancellationTokenSource();
            _ = this.DeathSequenceAsync(this.deathCts.Token);
        }

        private void AttachToPlayerFace()
        {
            Debug.Log("Attaching to headset...");

            // Shut down navMesh and Physics
            if (this.rootParent.TryGetComponent<UnityEngine.AI.NavMeshAgent>(out var agent))
            {
                agent.enabled = false;
            }

            if (this.rootParent.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.isKinematic = true;
                rb.detectCollisions = false;
            }

            // Find the main Camera of the player
            Transform headTransform = Camera.main != null ? Camera.main.transform : this.playerTransform;

            // Make the doll a child of the main camera
            this.rootParent.SetParent(headTransform);

            // Position the doll in front of the camera (Adjustable height and dept distance)
            this.rootParent.localPosition = new Vector3(0, this.verticalOffset, this.faceProximity);

            // Make the doll look at you.
            this.rootParent.localRotation = Quaternion.Euler(0, 180, 0);
        }

        private async Awaitable DeathSequenceAsync(CancellationToken ct)
        {
            await Awaitable.WaitForSecondsAsync(this.timeBeforeDeathCall, ct);

            if (!ct.IsCancellationRequested)
            {
                this.TriggerImmediateKill();
            }
        }

        private void TriggerImmediateKill()
        {
            this.KillPlayer();
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