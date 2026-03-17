using UnityEngine;

namespace MonsterSystem
{
    public class TeleportToTargetState : MonsterState
    {
        [SerializeField] private Transform target;

        public Transform Target { get => target; set => target = value; }

        public override void OnStateEnter()
        {
            if (target != null)
            {
                controller.transform.position = target.position;
                controller.transform.rotation = target.rotation;
            }
        }
    }
}
