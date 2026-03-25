using UnityEngine;

namespace MonsterSystem
{
    public class TeleportToTargetState : MonsterState
    {
        [SerializeField] private Transform target;

        public Transform Target { get => this.target; set => this.target = value; }

        public override void OnStateEnter()
        {
            if (this.target != null)
            {
                this.controller.transform.position = this.target.position;
                this.controller.transform.rotation = this.target.rotation;
            }
        }
    }
}
