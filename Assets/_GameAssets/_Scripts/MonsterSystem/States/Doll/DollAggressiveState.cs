using UnityEngine;
using UnityEngine.AI;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

namespace MonsterSystem
{
    public class DollAggressiveState : NavMeshMoveState
    {
        [Header("=== Doll Specifics ===")]
        [SerializeField] private Rigidbody rb;
        [SerializeField] private XRGrabInteractable grabInteractable;

        public override void OnStateEnter()
        {
            if (this.grabInteractable != null)
            {
                this.grabInteractable.enabled = false;
            }

            if (this.rb != null)
            {
                this.rb.linearVelocity = Vector3.zero;
                this.rb.angularVelocity = Vector3.zero;
                this.rb.isKinematic = true;
            }

            var agent = GetComponent<NavMeshAgent>();
            if (agent != null)
            {
                agent.enabled = true; // MUST be enabled for Warp to function

                NavMeshHit hit;
                if (NavMesh.SamplePosition(transform.position, out hit, 10.0f, NavMesh.AllAreas))
                {
                    agent.Warp(hit.position);
                }
                else
                {
                    Debug.LogWarning("[DollAggressiveState] Could not find a NavMesh close enough to snap to!");
                }
            }

            base.OnStateEnter();
            Debug.Log("Doll is Aggressive! Snapped to NavMesh and hunting the player.");
        }

        public override void OnStateExit()
        {
            base.OnStateExit();

            var agent = GetComponent<NavMeshAgent>();
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.enabled = false;
            }
        }
    }
}