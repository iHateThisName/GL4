using System;

namespace MonsterSystem
{
    [Serializable]
    public abstract class TransitionCondition : ITransitionCondition
    {
        public abstract bool Evaluate(MonsterController controller);
    }
}
