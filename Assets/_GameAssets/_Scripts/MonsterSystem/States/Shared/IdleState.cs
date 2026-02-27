using UnityEngine;

namespace MonsterSystem
{
    public class IdleState : MonsterState
    {
        [SerializeField] private string idleAnimTrigger;
        [SerializeField] private string timerKey = "idle";

        public override void OnStateEnter(MonsterController controller)
        {
            controller.ResetTimer(timerKey);

            MonsterAnimation.SetTrigger(controller.Animator, idleAnimTrigger);
        }

        public override void OnStateTick(MonsterController controller, float tickDelta)
        {
            controller.TickTimer(timerKey, tickDelta);
        }
    }
}
