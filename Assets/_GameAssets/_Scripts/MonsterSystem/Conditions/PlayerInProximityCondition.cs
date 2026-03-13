using UnityEngine;

namespace MonsterSystem
{
    [System.Serializable]
    public class PlayerInProximityCondition : TransitionCondition
    {
        [Tooltip("Optional sensor ID. Leave empty for first PlayerProximitySensor found.")]
        [SerializeField] private string sensorId = "";

        public override bool Evaluate(MonsterController controller)
        {
            PlayerProximitySensor sensor;

            if (string.IsNullOrEmpty(sensorId))
                sensor = controller.GetSensor<PlayerProximitySensor>();
            else
                sensor = controller.GetSensor<PlayerProximitySensor>(sensorId);

            return sensor != null && sensor.IsPlayerInRange;
        }
    }
}
