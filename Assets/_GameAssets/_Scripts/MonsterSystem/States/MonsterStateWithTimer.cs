using MonsterSystem;
using UnityEngine;

/// <summary>
/// A MonsterState that manages a timer via TimerHandle.
/// Provides tick and finished callbacks for subclasses to override.
/// </summary>
public class MonsterStateWithTimer : MonsterState
{
    [Header("=== Timer Configuration ===")]
    [SerializeField] private float interval = 0.1f;
    [SerializeField] private float duration = 0f;

    private TimerHandle timerHandle;

    public override void Initialize(MonsterController owningController)
    {
        base.Initialize(owningController);
        this.timerHandle = TimerManager.Create(this.interval, this.duration);
        TimerManager.SetCallbacks(this.timerHandle, this.OnTimerTick, this.OnTimerFinished);
        // Pause immediately — OnStateEnter will resume
        TimerManager.Pause(this.timerHandle);
    }

    public override void OnStateEnter()
    {
        if (TimerManager.Validate(this.timerHandle))
        {
            ref var t = ref TimerManager.GetRef(this.timerHandle);
            t.Elapsed = 0f;
            t.NextInterval = t.Interval;
            t.IsFinished = 0;
            t.IsRunning = 1;
        }
    }

    public override void OnStateExit()
    {
        TimerManager.Pause(this.timerHandle);
    }

    protected virtual void OnTimerTick() { }
    protected virtual void OnTimerFinished() { }

    public float GetTime() => TimerManager.Validate(this.timerHandle) ? TimerManager.GetRef(this.timerHandle).Elapsed : 0f;

    public TimerHandle GetTimerHandle() => this.timerHandle;
}
