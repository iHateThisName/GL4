using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using static UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics.HapticsUtility;

namespace MonsterSystem
{
    public class DollAttackState : MonsterState
    {
        [Header("Jumpscare Positioning")]
        [Tooltip("How close the doll's center is to the player's eyes.")]
        [SerializeField] private float faceProximity = 0.2f;
        [Tooltip("Slight vertical offset if the doll is too high/low relative to its pivot.")]
        [SerializeField] private float verticalOffset = -0.1f;

        [Header("Timing")]
        [Tooltip("Seconds the doll is attached to the face before the final death screen fades in.")]
        [SerializeField] private float timeBeforeDeathCall = 1.0f;

        // References
        private DollSensor sensor;
        //private NavMeshAgent navAgent;

        [SerializeField] private SO_RuntimeReferences runtimeReferences;

        private Transform playerTransform;
        private Transform rootParent;

        public override void Initialize(MonsterController controller)
        {
            base.Initialize(controller);
            this.sensor = controller.GetSensor<DollSensor>();
            //navAgent = controller.GetComponent<NavMeshAgent>();
            this.playerTransform = this.runtimeReferences?.Player;
            this.rootParent = controller.transform;
        }

        public override void OnStateEnter()
        {
            this.playerTransform = this.runtimeReferences?.Player;
            Debug.Log("THEDOLLHASYOU.");

            // 1. Stop all movement on the root object
            //if (navAgent != null)
            //{
            //    navAgent.isStopped = true;
            //    navAgent.enabled = false; // Completely disable to stop interference
            //}

            // 2. Attach to player's face
            if (this.runtimeReferences != null)
            {
                AttachToPlayerFace();
            }
            else
            {
                Debug.LogError("DollAttackState: Player Transform not found on DollSensor! Triggering emergency kill.");
                TriggerImmediateKill();
                return;
            }

            // 3. Optional: Trigger attack animation/screamer sound here
            // Controller.Animator.SetTrigger("Jumpscare");
            // Controller.Audio.PlayOneShot(jumpscareSound);

            // 4. Start the countdown to the actual Game Over screen
            StartCoroutine(DeathSequence());
        }

        private void AttachToPlayerFace()
        {
            Debug.Log("attach");
            // IMPORTANT: In VR, parenting to the 'head' transform can cause slight latency jitter.
            // For a jumpscare, it's often better to disable the visual mesh renderer of the 
            // walking doll and enable a specific 'jumpscare mesh' that is ALREADY a child of the VR Camera.
            //
            // However, to keep it simple and fulfill the specific request:

            // Make the doll object a child of the player's camera
            this.rootParent.SetParent(this.playerTransform);

            // Position it exactly in front of the "eyes"
            // Vector3.forward is relative to the player's view direction
            this.rootParent.localPosition = (Vector3.forward * faceProximity) + (Vector3.up * verticalOffset);

            // Rotate it to look directly back at the player
            this.rootParent.localRotation = Quaternion.Euler(0, 180, 0);

            // Note: Depending on your doll model's pivot point, you may need to tweak localRotation or verticalOffset.
        }

        private IEnumerator DeathSequence()
        {
            yield return new WaitForSeconds(timeBeforeDeathCall);
            TriggerImmediateKill();
        }

        private void TriggerImmediateKill()
        {
            // Call your static external death system
            DeathSystem.KillPlayer(DeathSystem.DeathEvent.DeathReason.Monster);
        }

        public override void OnStateExit()
        {
            // Since the player is dead, OnStateExit isn't strictly necessary, 
            // but for cleanup if you test in editor without restarting:
            StopAllCoroutines();
        }
    }
}