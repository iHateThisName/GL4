namespace MonsterSystem
{
    public interface IMonsterState
    {
        void OnStateEnter(MonsterController controller);
        void OnStateTick(MonsterController controller, float tickDelta);
        void OnStateExit(MonsterController controller);
    }
}
