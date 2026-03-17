using System;
using UnityEngine;

/// <summary>
/// A reusable timer class that integrates with Unity's PlayerLoopSystem via TimerManager.
/// Supports both finite durations and infinite running, with periodic tick events.
/// </summary>
/// <remarks>
/// Timers are automatically registered with TimerManager when started and deregistered when disposed.
/// Always call Dispose() when done with a timer to prevent memory leaks.
/// </remarks>
public class Timer : IDisposable
{
    /// <summary>Fired once when the timer starts.</summary>
    public event Action OnTimerStart = delegate { };
    /// <summary>Fired every time the tick interval elapses.</summary>
    public event Action OnTimerTick = delegate { };
    /// <summary>Fired once when the timer reaches its duration (finite timers only).</summary>
    public event Action OnTimerFinished = delegate { };

    public float Elapsed { get; protected set; } // Time since start in seconds
    public float Interval { get; protected set; } // Current tick interval in seconds
    public float Duration { get; protected set; } // Total duration in seconds, 0 = run forever
    public bool IsRunning { get; private set; }
    public bool IsFinished => this.Duration > 0 && this.Elapsed >= this.Duration;

    /// <summary>Returns progress from 0 to 1 for finite timers.</summary>
    public float Progress => Mathf.Clamp(this.Elapsed / this.Duration, 0, 1);

    private float nextInterval; // Elapsed time at which next tick should fire
    private bool disposed;
    private bool intervalWasSetDuringTick; // Prevents double-advancing when SetInterval is called in tick handler

    /// <summary>
    /// Creates a new timer with the specified tick interval and optional duration.
    /// </summary>
    /// <param name="interval">Seconds between ticks.</param>
    /// <param name="duration">Total runtime before OnTimerFinished fires. 0 = infinite.</param>
    public Timer(float interval, float duration = 0f)
    {
        this.Interval = interval;
        this.Duration = duration;
        this.nextInterval = interval;
    }

    /// <summary>
    /// Finalizer ensures cleanup if Dispose() was not called.
    /// </summary>
    ~Timer() => Dispose(false);

    /// <summary>
    /// Changes the tick interval mid-run.
    /// Useful for dynamic intervals like flashlight flicker randomization.
    /// </summary>
    /// <param name="newInterval">The new interval in seconds.</param>
    public void SetInterval(float newInterval)
    {
        this.Interval = newInterval;
        this.nextInterval = Elapsed + this.Interval;
        this.intervalWasSetDuringTick = true;
    }

    /// <summary>
    /// Starts the timer and registers it with TimerManager.
    /// Does nothing if already running, finished, or disposed.
    /// </summary>
    public void Start()
    {
        if (this.disposed || this.IsFinished || this.IsRunning) return;

        this.IsRunning = true;
        TimerManager.RegisterTimer(this);
        this.OnTimerStart.Invoke();
    }

    /// <summary>
    /// Pauses the timer without resetting elapsed time.
    /// </summary>
    public void Pause() => this.IsRunning = false;

    /// <summary>
    /// Resumes a paused timer.
    /// </summary>
    public void Resume() => this.IsRunning = true;

    /// <summary>
    /// Resets elapsed time to zero and optionally restarts the timer.
    /// </summary>
    /// <param name="restart">If true, immediately starts the timer after reset.</param>
    public void ResetTimer(bool restart = false)
    {
        this.Elapsed = 0f;
        this.nextInterval = this.Interval;
        this.IsRunning = false;
        if (restart) Start();
    }

    /// <summary>
    /// Called by TimerManager each frame. Updates elapsed time and fires tick/finished events.
    /// </summary>
    public void Update()
    {
        if (!this.IsRunning || this.IsFinished) return;

        this.intervalWasSetDuringTick = false;
        this.Elapsed += Time.deltaTime;

        // Fire tick events, catching up if frame took longer than interval
        while (this.Elapsed >= this.nextInterval && this.nextInterval > 0)
        {
            this.OnTimerTick?.Invoke();
            if (!this.intervalWasSetDuringTick)
                this.nextInterval += this.Interval;
        }

        // Check if duration reached (finite timers only)
        if (this.Duration > 0f && this.Elapsed >= this.Duration)
        {
            this.IsRunning = false;
            this.OnTimerFinished?.Invoke();
        }
    }
    
    public void UpdateFixed(float deltaTime)                                                                                                                                          
    {                                                                                                                                                                                 
        if (!this.IsRunning || this.IsFinished) return;

        this.intervalWasSetDuringTick = false;
        this.Elapsed += deltaTime;

        // Fire tick events, catching up if frame took longer than interval
        while (this.Elapsed >= this.nextInterval && this.nextInterval > 0)
        {
            this.OnTimerTick?.Invoke();
            if (!this.intervalWasSetDuringTick)
                this.nextInterval += this.Interval;
        }

        // Check if duration reached (finite timers only)
        if (this.Duration > 0f && this.Elapsed >= this.Duration)
        {
            this.IsRunning = false;
            this.OnTimerFinished?.Invoke();
        }
    } 

    /// <summary>
    /// Disposes the timer, deregistering from TimerManager and clearing all event listeners.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Internal dispose implementation following the dispose pattern.
    /// </summary>
    /// <param name="disposing">True if called from Dispose(), false if from finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (this.disposed) return;

        if (disposing)
        {
            TimerManager.DeregisterTimer(this);
            ClearAllEvents();
        }
        this.disposed = true;
    }

    /// <summary>
    /// Clears all event subscribers to prevent memory leaks.
    /// </summary>
    public void ClearAllEvents()
    {
        this.OnTimerStart = delegate { };
        this.OnTimerTick = delegate { };
        this.OnTimerFinished = delegate { };
    }
}
