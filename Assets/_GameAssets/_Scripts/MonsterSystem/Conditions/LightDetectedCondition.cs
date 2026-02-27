namespace MonsterSystem
{
    [System.Serializable]
    public class LightDetectedCondition : TransitionCondition
    {
        public override bool Evaluate(MonsterController controller)
        {
            var sensor = controller.GetSensor<LightSensor>();
            return sensor != null && sensor.IsLitUp;
        }
    }
}
