using MonsterSystem;
using UnityEngine;

public class MonsterStateWithTimer : MonsterState
{
    [Header("=== Timer Configuration ===")]
    [SerializeField] private float interval = 0.1f;
    [SerializeField] private float duration = 0f;
    
    private Timer timer;

    public override void OnStateEnter()
    {
        this.timer = new Timer(this.interval, this.duration);
        this.timer.Start();
    }

    public override void OnStateExit()
    {
        this.timer.Pause();
        this.timer.Dispose();
    }
    
    protected virtual void OnTimerTick() { }
    
    protected virtual void OnTimerFinished() { }

    public float GetTime() => this.timer != null ? this.timer.Elapsed : 0;
    
    public Timer GetTimer() => this.timer;
}