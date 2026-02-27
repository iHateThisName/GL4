using UnityEngine;

namespace MonsterSystem
{
    public abstract class MonsterSensor : MonoBehaviour, IMonsterSensor
    {
        [SerializeField] private string sensorId = "";

        public string SensorId => sensorId;

        /// Called by MonsterStateManager during the tick (not per-frame).
        public virtual void Tick(MonsterController controller) { }
    }
}
