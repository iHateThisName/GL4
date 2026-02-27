using UnityEngine;

namespace MonsterSystem
{
    /// Drains satiety independently using real time. Writes to controller timer dict.
    public class SatietySensor : MonsterSensor
    {
        [SerializeField] private float maxSatiety = 60f;

        public float CurrentSatiety { get; private set; }
        public float DrainMultiplier { get; set; } = 1f;

        private float lastTickTime;

        private void OnEnable()
        {
            CurrentSatiety = maxSatiety;
            lastTickTime = Time.time;
        }

        public override void Tick(MonsterController controller)
        {
            float now = Time.time;
            float delta = now - lastTickTime;
            lastTickTime = now;

            var config = controller.GetConfig<MunchConfig>();
            if (config == null) return;

            float rate = config.satietyDrainRate;

            var nightOverride = controller.Config.GetOverrideForNight(controller.CurrentNight);
            rate *= nightOverride.aggressionMultiplier;

            var radioSensor = controller.GetSensor<RadioSensor>();
            if (radioSensor != null)
                rate *= radioSensor.CurrentAggressionModifier;

            rate *= DrainMultiplier;

            CurrentSatiety = Mathf.Clamp(CurrentSatiety - rate * delta, 0f, maxSatiety);
            controller.SetTimer("satiety", CurrentSatiety);
        }

        /// Add (or subtract) satiety and sync to controller timer.
        public void AddSatiety(float amount, MonsterController controller)
        {
            CurrentSatiety = Mathf.Clamp(CurrentSatiety + amount, 0f, maxSatiety);
            controller.SetTimer("satiety", CurrentSatiety);
        }
    }
}
