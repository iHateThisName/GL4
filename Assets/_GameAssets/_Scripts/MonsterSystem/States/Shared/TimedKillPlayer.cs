using UnityEngine;

public class TimedKillPlayer : MonsterStateWithTimer
{
    public override void OnStateEnter()
    {
        base.OnStateEnter();
        Debug.Log("TimedKillPlayer");
    }

    protected override void OnTimerFinished()
    {
        base.OnTimerFinished();
        KillPlayer();
    }
}