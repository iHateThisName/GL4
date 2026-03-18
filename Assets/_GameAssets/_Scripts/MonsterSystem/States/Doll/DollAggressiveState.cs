using UnityEngine;
using UnityEngine.AI;

namespace MonsterSystem
{
    public class DollAggressiveState : MonsterState
    {
        private NavMeshAgent navAgent;
        private DollSensor sensor;

        public override void Initialize(MonsterController controller)
        {
            base.Initialize(controller);
            navAgent = controller.GetComponent<NavMeshAgent>();
            sensor = controller.GetSensor<DollSensor>();
        }

        public override void OnStateEnter()
        {
            Debug.Log("Doll is Aggressive! Standing and Chasing!");
            navAgent.isStopped = false;
            // Controller.Animator.SetTrigger("StandUp");
        }

        private void Update()
        {
            // The state purely handles movement. 
            // The Sensor is simultaneously running OnTick() to check if distance <= attackDistance.
            if (sensor != null && sensor.playerTransform != null)
            {
                navAgent.SetDestination(sensor.playerTransform.position);
            }
        }

        public override void OnStateExit()
        {
            navAgent.isStopped = true;
        }
    }
}