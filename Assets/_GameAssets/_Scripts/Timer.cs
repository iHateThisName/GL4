using System;
using UnityEngine;

public class Timer : IDisposable
{
    public event Action OnTimerStart = delegate { };
    /// <summary>Fired every time the tick interval elapses.</summary>
    public event Action OnTimerTick = delegate { };
    public event Action OnTimerFinished = delegate { };
    
    public float Elapsed      { get; protected set; }   // time since last tick
    public float Interval     { get; protected set; }   // current tick interval
    public float Duration     { get; protected set; }   // 0 = run forever
    public bool  IsRunning    { get; private set; }
    public bool  IsFinished => Duration > 0 && Elapsed >= Duration;
    
    public float Progress => Mathf.Clamp(Elapsed / Duration, 0, 1);
    
    private  bool disposed;
    
    /// <param name="interval">Seconds between ticks.</param>
    /// <param name="duration">Total runtime before OnFinished fires. 0 = infinite.</param>
    /// <param name="startNow">If true, starts running immediately.</param>
    public Timer(float interval, float duration = 0f)
    {
        Interval = interval;
        Duration = duration;
        disposed = false;
    }
    
    // Destructor
    ~Timer() => Dispose(false);
    
    /// <summary>Change the tick interval mid-run (e.g. flashlight randomization).</summary>
    public void SetInterval(float newInterval)
    {
        Interval = newInterval;
        Elapsed   = 0f;   // reset current tick progress
    }

    public void Start()
    {
        if (disposed || IsFinished) return;
        if (IsRunning) return;
        
        IsRunning = true;
        TimerManager.RegisterTimer(this);
        OnTimerStart.Invoke();
    }

    public void Pause() => IsRunning = false;

    public void Resume() => IsRunning = true;

    public void ResetTimer(bool restart = false)
    {
        Elapsed = 0f;
        IsRunning = false;
        if (restart) Start();
    }
    
    public void Update()
    {
        if (!IsRunning || IsFinished) return;

        Elapsed += Time.deltaTime;

        // Check total duration cap
        if (Duration > 0f && Elapsed >= Duration)
        {
            IsRunning = false;
            OnTimerFinished?.Invoke();
            return;
        }

        // Check tick interval
        if (Elapsed >= Interval)
        {
            Elapsed -= Interval;
            OnTimerTick?.Invoke();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    protected virtual void Dispose(bool disposing) 
    {
        if (disposed) return;

        if (disposing) 
        {
            TimerManager.DeregisterTimer(this);
            ClearAllEvents();
        }

        disposed = true;
    }
    
    private void ClearAllEvents()
    {
        OnTimerStart    = delegate { };
        OnTimerTick     = delegate { };
        OnTimerFinished = delegate { };
    }
}