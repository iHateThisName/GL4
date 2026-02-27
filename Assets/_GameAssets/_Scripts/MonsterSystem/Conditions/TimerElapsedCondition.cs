using UnityEngine;

namespace MonsterSystem
{
    [System.Serializable]
    public class TimerElapsedCondition : TransitionCondition
    {
        [SerializeField] private string timerKey;
        [SerializeField] private float threshold;

        public override bool Evaluate(MonsterController controller)
        {
            return controller.GetTimer(timerKey) >= threshold;
        }
    }
}
