using UnityEngine;

namespace MonsterSystem
{
    [System.Serializable]
    public class RadioAggressionCondition : TransitionCondition
    {
        [SerializeField] private float threshold = 2f;

        public override bool Evaluate(MonsterController controller)
        {
            var sensor = controller.GetSensor<RadioSensor>();
            return sensor != null && sensor.CurrentAggressionModifier >= threshold;
        }
    }
}
