namespace MonsterSystem
{
    [System.Serializable]
    public class PlayerGazingCondition : TransitionCondition
    {
        public override bool Evaluate(MonsterController controller)
        {
            var sensor = controller.GetSensor<GazeSensor>();
            return sensor != null && sensor.IsBeingObserved;
        }
    }
}
