using UnityEngine;

namespace MonsterSystem
{
    public abstract class MonsterState : MonoBehaviour, IMonsterState
    {
        /// Called when this state becomes active.
        public virtual void OnStateEnter(MonsterController controller) { }

        /// Called every tick by MonsterStateManager (NOT every frame).
        public virtual void OnStateTick(MonsterController controller, float tickDelta) { }

        /// Called when leaving this state for another.
        public virtual void OnStateExit(MonsterController controller) { }

        /// Helper: request an imperative transition from within a state.
        protected void RequestTransition(MonsterController controller, MonsterState targetState)
        {
            MonsterStateManager.RequestTransition(controller, targetState);
        }
    }
}
