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

    private Timer timer; // The managed Timer instance created each time the state is entered

    /// <summary>
    /// Creates a new Timer with the configured interval and duration,
    /// subscribes to its events, and starts it.
    /// </summary>
    public override void OnStateEnter()
    {
        // Instantiate a fresh timer and wire up event callbacks
        this.timer = new Timer(this.interval, this.duration);
        this.timer.OnTimerTick += this.OnTimerTick;
        this.timer.OnTimerFinished += this.OnTimerFinished;
        this.timer.Start();
    }

    /// <summary>
    /// Pauses and disposes the timer when exiting this state to prevent leaked subscriptions.
    /// </summary>
    public override void OnStateExit()
    {
        // Guard against double-disposal if the timer was never created
        if (this.timer == null) return;
        this.timer.Pause();
        this.timer.Dispose();
        this.timer = null;
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
