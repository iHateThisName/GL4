using UnityEngine;

namespace MonsterSystem
{
    public enum ComparisonType { Greater, Less, GreaterEqual, LessEqual, Equal }

    [System.Serializable]
    public class NumberComparisonCondition : TransitionCondition
    {
        [SerializeField] private string key;
        [SerializeField] private ComparisonType comparison;
        [SerializeField] private float value;

        public override bool Evaluate(MonsterController controller)
        {
            float current = controller.GetTimer(key);
            return comparison switch
            {
                ComparisonType.Greater => current > value,
                ComparisonType.Less => current < value,
                ComparisonType.GreaterEqual => current >= value,
                ComparisonType.LessEqual => current <= value,
                ComparisonType.Equal => Mathf.Approximately(current, value),
                _ => false
            };
        }
    }
}
