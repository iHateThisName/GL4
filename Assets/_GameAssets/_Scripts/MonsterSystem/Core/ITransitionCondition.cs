namespace MonsterSystem
{
    public interface ITransitionCondition
    {
        bool Evaluate(MonsterController controller);
    }
}
