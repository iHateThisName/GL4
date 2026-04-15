using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MonsterSystem
{
    public class DollRelocateState : MonsterState
    {
        [Header("=== Doll Specifics ===")]
        [SerializeField] private XRGrabInteractable grabInteractable;
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private bool canKillMomentum = true;

        [Header("=== Relocation Config ===")]
        [Tooltip("If true, snaps to the nearest point. If false, picks a random non-repeating point.")]
        [SerializeField] private bool useNearest = true;
        [SerializeField] private SO_TransformCollection spawnPoints;

        [Header("=== Horror Elements ===")]
        [Tooltip("How long she waits in the dark before the chase begins.")]
        [SerializeField] private float jumpScareDelay = 3.0f;
        [Tooltip("Drag the doll's main mesh renderer here so she can turn invisible")]
        [SerializeField] private Renderer dollRenderer;

        [Header("=== Next State ===")]
        [Tooltip("Drag the Aggressive State here")]
        [SerializeField] private MonsterState nextState;

        private Rigidbody rb;
        private int lastIndex = -1;
        private Coroutine delayRoutine;

        public override void Initialize(MonsterController owningController)
        {
            base.Initialize(owningController);
            this.rb = this.controller.GetComponent<Rigidbody>();
        }

        public override void OnStateEnter()
        {

            base.OnStateEnter();
            this.TriggerAffordances<AudioAffordance>();
            this.TriggerAffordances <VfxAffordance>();

            // 1. Lock down interaction
            if (this.grabInteractable != null)
            {
                this.grabInteractable.enabled = false;
            }

            // 2. Handle Physics
            if (this.rb != null)
            {
                if (this.canKillMomentum)
                {
                    this.rb.linearVelocity = Vector3.zero;
                    this.rb.angularVelocity = Vector3.zero;
                }
                this.rb.isKinematic = true;
            }

            // 3. Prepare NavMeshAgent and turn OFF visibility!
            if (this.agent != null)
            {
                this.agent.enabled = false;
            }
            this.SetVisibility(false);

            // 4. Determine Target Spot
            var bestSpot = this.GetTargetSpawnPoint();

            // 5. Teleport & Warp
            if (bestSpot.position != Vector3.zero)
            {
                if (NavMesh.SamplePosition(bestSpot.position, out NavMeshHit hit, 5.0f, NavMesh.AllAreas))
                {
                    this.controller.transform.SetPositionAndRotation(hit.position, Quaternion.Euler(bestSpot.rotation));

                    if (this.agent != null)
                    {
                        this.agent.enabled = true;
                        this.agent.Warp(hit.position);
                    }
                }
                else
                {
                    Debug.LogError($"[DollRelocate] FAILED to find NavMesh near {bestSpot}! Forcing transform.");
                    this.controller.transform.SetPositionAndRotation(bestSpot.position, Quaternion.Euler(bestSpot.rotation));

                    if (this.agent != null)
                    {
                        this.agent.enabled = true;
                    }
                }
            }

            // 6. Start the Suspense Timer!
            if (this.agent != null && this.agent.isOnNavMesh)
            {
                this.delayRoutine = StartCoroutine(this.WaitAndChaseRoutine());
            }
            else
            {
                Debug.LogError("[DollRelocate] FATAL: Agent failed to snap to NavMesh. Blocking chase to prevent crash.");
                this.SetVisibility(true); // Failsafe if it crashes
            }
        }

        private IEnumerator WaitAndChaseRoutine()
        {
            // Pause the execution here while she is invisible
            yield return new WaitForSeconds(this.jumpScareDelay);

            // Pop her back into existence!
            this.SetVisibility(true);

            // Time is up, hand control over to the aggressive chase state
            if (this.nextState != null)
            {
                this.RequestTransition(this.nextState);
            }
        }

        public override void OnStateExit()
        {
            base.OnStateExit();

            // Safety cleanup: If the state is forced to exit early, kill the timer
            if (this.delayRoutine != null)
            {
                StopCoroutine(this.delayRoutine);
                this.delayRoutine = null;
            }

            // Failsafe: Guarantee she is visible when leaving this state
            this.SetVisibility(true);
        }

        // Updated to handle just the one renderer
        private void SetVisibility(bool isVisible)
        {
            if (this.dollRenderer != null)
            {
                this.dollRenderer.enabled = isVisible;
            }
        }

        private SpawnPoint GetTargetSpawnPoint()
        {
            if (this.spawnPoints == null || this.spawnPoints.points.Length == 0)
            {
                return new SpawnPoint();
            }

            if (this.useNearest)
            {
                var nearest = new SpawnPoint();
                float minDistance = float.MaxValue;
                Vector3 currentPos = this.controller.transform.position;

                foreach (var point in this.spawnPoints.points)
                {
                    float distance = Vector3.Distance(currentPos, point.position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        nearest = point;
                    }
                }
                return nearest;
            }
            else
            {
                int index;
                if (this.spawnPoints.points.Length == 1)
                {
                    index = 0;
                }
                else
                {
                    do
                    {
                        index = Random.Range(0, this.spawnPoints.points.Length);
                    }
                    while (index == this.lastIndex);
                }

                this.lastIndex = index;
                return this.spawnPoints.points[index];
            }
        }
    }
}