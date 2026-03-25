using MonsterSystem;
using UnityEngine;

/// <summary>
/// A MonsterState that creates and manages a Timer on enter,
/// providing tick and finished callbacks for subclasses to override.
/// </summary>
public class MonsterStateWithTimer : MonsterState
{
    [Header("=== Timer Configuration ===")]
    [SerializeField] private float interval = 0.1f; // Seconds between each timer tick
    [SerializeField] private float duration = 0f; // Total timer duration; 0 means infinite

    private Timer timer; // The managed Timer instance, created once in Initialize and reused across state entries
    private bool timerRegistered; // Tracks whether the timer has been registered with TimerManager

    /// <summary>
    /// Creates the Timer once during initialization and wires up event callbacks.
    /// </summary>
    public override void Initialize(MonsterController owningController)
    {
        base.Initialize(owningController);
        this.timer = new Timer(this.interval, this.duration);
        this.timer.OnTimerTick += this.OnTimerTick;
        this.timer.OnTimerFinished += this.OnTimerFinished;
    }

    /// <summary>
    /// Resets and starts/resumes the existing timer when entering this state.
    /// First entry calls Start() to register with TimerManager; subsequent entries just reset and resume.
    /// </summary>
    public override void OnStateEnter()
    {
        if (!this.timerRegistered)
        {
            this.timer.Start();
            this.timerRegistered = true;
        }
        else
        {
            this.timer.ResetTimer();
            this.timer.Resume();
        }
    }

    /// <summary>
    /// Pauses the timer when exiting this state.
    /// </summary>
    public override void OnStateExit()
    {
        if (this.timer == null) return;
        this.timer.Pause();
    }

    /// <summary>
    /// Called on every timer tick. Override in subclasses to add periodic behaviour.
    /// </summary>
    protected virtual void OnTimerTick() { }

    /// <summary>
    /// Called when the timer reaches its full duration. Override in subclasses to handle completion.
    /// </summary>
    protected virtual void OnTimerFinished() { }

    /// <summary>
    /// Returns the elapsed time of the current timer, or 0 if no timer exists.
    /// </summary>
    public float GetTime() => this.timer != null ? this.timer.Elapsed : 0;

    /// <summary>
    /// Returns the underlying Timer instance, or null if the state is not active.
    /// </summary>
    public Timer GetTimer() => this.timer;
}
