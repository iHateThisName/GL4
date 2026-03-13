namespace MonsterSystem
{
    [System.Serializable]
    public class PlayerOutdoorsCondition : TransitionCondition
    {
        public override bool Evaluate(MonsterController controller)
        {
            if (PlayerTemperatureSimulator.Instance == null) return false;
            return PlayerTemperatureSimulator.Instance.CurrentLocationType
                == PlayerTemperatureSimulator.EnumLocationType.Cold;
        }
    }
}
