namespace MonsterSystem
{
    public class RadioSensor : MonsterSensor
    {
        public float CurrentAggressionModifier { get; private set; } = 1f;
        public bool IsDangerMode { get; private set; }

        public override void OnTick(float tickDelta)
        {
            base.OnTick(tickDelta);

            CurrentAggressionModifier = RadioBroadcast.AggressionMultiplier;
            IsDangerMode = RadioBroadcast.IsDangerMode;
        }
    }
}
