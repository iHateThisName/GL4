using System;
using UnityEngine;

public class Timer : IDisposable
{
    public event Action OnTimerStart = delegate { };
    /// <summary>Fired every time the tick interval elapses.</summary>
    public event Action OnTimerTick = delegate { };
    public event Action OnTimerFinished = delegate { };
    
    public float Elapsed      { get; protected set; }   // time since start
    public float Interval     { get; protected set; }   // current tick interval
    public float Duration     { get; protected set; }   // 0 = run forever
    public bool  IsRunning    { get; private set; }
    public bool  IsFinished => this.Duration > 0 && this.Elapsed >= this.Duration;
    
    public float Progress => Mathf.Clamp(this.Elapsed / this.Duration, 0, 1);

    private float nextInterval;
    private bool disposed;
    private bool intervalWasSetDuringTick; // Track if SetInterval was called

    
    /// <param name="interval">Seconds between ticks.</param>
    /// <param name="duration">Total runtime before OnFinished fires. 0 = infinite.</param>
    /// <param name="startNow">If true, starts running immediately.</param>
    public Timer(float interval, float duration = 0f)
    {
        this.Interval = interval;
        this.Duration = duration;
        this.nextInterval = interval;
    }
    
    // Destructor
    ~Timer() => Dispose(false);
    
    /// <summary>Change the tick interval mid-run (e.g. flashlight randomization).</summary>
    public void SetInterval(float newInterval)
    {
        this.Interval = newInterval;
        this.nextInterval = Elapsed + this.Interval;
        this.intervalWasSetDuringTick = true;
    }

    public void Start()
    {
        if (this.disposed || this.IsFinished || this.IsRunning) return;
        
        this.IsRunning = true;
        TimerManager.RegisterTimer(this);
        OnTimerStart.Invoke();
    }

    public void Pause() => this.IsRunning = false;

    public void Resume() => this.IsRunning = true;

    public void ResetTimer(bool restart = false)
    {
        this.Elapsed = 0f;
        this.IsRunning = false;
        if (restart) Start();
    }
    
    public void Update()
    {
        if (!this.IsRunning || this.IsFinished) return;

        this.Elapsed += Time.deltaTime;

        
        // Check tick interval
        if (this.Elapsed >= this.nextInterval)
        {
            OnTimerTick?.Invoke();
            if (!this.intervalWasSetDuringTick)
            {
                this.nextInterval = this.Elapsed + this.Interval;
            }
        }
        
        // Check total duration cap
        if (this.Duration > 0f && this.Elapsed >= this.Duration)
        {
            this.IsRunning = false;
            OnTimerFinished?.Invoke();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
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
    
    private void ClearAllEvents()
    {
        OnTimerStart    = delegate { };
        OnTimerTick     = delegate { };
        OnTimerFinished = delegate { };
    }
}