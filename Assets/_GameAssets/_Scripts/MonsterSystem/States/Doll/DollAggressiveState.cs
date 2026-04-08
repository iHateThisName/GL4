using UnityEngine;
using UnityEngine.AI;

namespace MonsterSystem
{
    public class DollAggressiveState : NavMeshMoveState
    {
        public override void OnStateEnter()
        {
            // The Relocate state already handled the teleporting, locked the physics, 
            // and perfectly snapped the NavMeshAgent to the floor.

            // We just let the base class hook up the timers and start the chase!
            base.OnStateEnter();

            Debug.Log("Doll is Aggressive and actively hunting!");
        }

        public override void OnStateExit()
        {
            // Let the base class handle its own cleanup first
            base.OnStateExit();

            // We grab the agent dynamically here to completely avoid that serialization error.
            // This safely stops her if she transitions to another state (like attacking or hiding).
            var navAgent = GetComponent<NavMeshAgent>();
            if (navAgent != null && navAgent.isOnNavMesh)
            {
                navAgent.isStopped = true;
            }
        }
    }
}