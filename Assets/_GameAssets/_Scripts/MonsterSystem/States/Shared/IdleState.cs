using UnityEngine;

namespace MonsterSystem
{
    public class IdleState : MonsterState
    {
        [SerializeField] private string idleAnimTrigger;

        public override void OnStateEnter()
        {
            MonsterAnimation.SetTrigger(controller.Animator, idleAnimTrigger);
        }

        public override void OnStateTick(float tickDelta)
        {
        }
    }
}
