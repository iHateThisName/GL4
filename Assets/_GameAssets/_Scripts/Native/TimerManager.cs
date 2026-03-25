using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

/// <summary>
/// Zero-allocation timer manager using NativeList + Burst.
/// Timers are blittable structs stored in a flat array, ticked by a Burst job.
/// Managed callbacks are dispatched on the main thread after the job completes.
/// </summary>
public static class TimerManager
{
    private static NativeList<TimerData> timers;
    private static NativeList<int> versions;
    private static NativeQueue<int> freeSlots;

    // Managed parallel arrays for callbacks (cannot be in Burst)
    private static Action[] onTick;
    private static Action[] onFinished;
    private static int capacity;
    private static bool initialized;

    private const int DEFAULT_CAPACITY = 32;

    public static void Initialize(int initialCapacity = DEFAULT_CAPACITY)
    {
        if (initialized) return;

        capacity = initialCapacity;
        timers = new NativeList<TimerData>(capacity, Allocator.Persistent);
        versions = new NativeList<int>(capacity, Allocator.Persistent);
        freeSlots = new NativeQueue<int>(Allocator.Persistent);
        onTick = new Action[capacity];
        onFinished = new Action[capacity];
        initialized = true;
    }

    /// <summary>
    /// Creates a new timer and returns a handle to it. The timer starts running immediately.
    /// </summary>
    public static TimerHandle Create(float interval, float duration = 0f)
    {
        if (!initialized) Initialize();

        int index;
        if (freeSlots.TryDequeue(out int freeIndex))
        {
            index = freeIndex;
        }
        else
        {
            index = timers.Length;
            timers.Add(default);
            versions.Add(0);
            GrowManagedArrays(index);
        }

        timers[index] = new TimerData
        {
            Interval = interval,
            Duration = duration,
            NextInterval = interval,
            IsRunning = 1
        };
        versions[index] = versions[index] + 1;

        return new TimerHandle(index, versions[index]);
    }

    /// <summary>
    /// Sets the tick and finished callbacks for a timer.
    /// </summary>
    public static void SetCallbacks(TimerHandle handle, Action tick, Action finished)
    {
        if (!Validate(handle)) return;
        onTick[handle.Index] = tick;
        onFinished[handle.Index] = finished;
    }

    /// <summary>
    /// Clears callbacks without releasing the timer slot.
    /// </summary>
    public static void ClearCallbacks(TimerHandle handle)
    {
        if (!Validate(handle)) return;
        onTick[handle.Index] = null;
        onFinished[handle.Index] = null;
    }

    /// <summary>
    /// Reconfigures an existing timer with new interval/duration and resets it.
    /// Avoids release+create churn. Clears callbacks so new ones must be set.
    /// </summary>
    public static void Reconfigure(TimerHandle handle, float interval, float duration = 0f)
    {
        if (!Validate(handle)) return;

        timers[handle.Index] = new TimerData
        {
            Interval = interval,
            Duration = duration,
            NextInterval = interval,
            IsRunning = 1
        };
        onTick[handle.Index] = null;
        onFinished[handle.Index] = null;
    }

    /// <summary>
    /// Releases a timer slot back to the pool.
    /// </summary>
    public static void Release(ref TimerHandle handle)
    {
        if (!Validate(handle)) return;
        timers[handle.Index] = default;
        onTick[handle.Index] = null;
        onFinished[handle.Index] = null;
        freeSlots.Enqueue(handle.Index);
        handle = TimerHandle.Invalid;
    }

    public static void Pause(TimerHandle handle)
    {
        if (!Validate(handle)) return;
        var t = timers[handle.Index];
        t.IsRunning = 0;
        timers[handle.Index] = t;
    }

    public static void Resume(TimerHandle handle)
    {
        if (!Validate(handle)) return;
        var t = timers[handle.Index];
        t.IsRunning = 1;
        timers[handle.Index] = t;
    }

    /// <summary>
    /// Returns a reference to the timer data for direct read/write.
    /// </summary>
    public static ref TimerData GetRef(TimerHandle handle) => ref timers.ElementAt(handle.Index);

    /// <summary>
    /// Returns true if the handle points to a valid, live timer slot.
    /// </summary>
    public static bool Validate(TimerHandle handle)
    {
        return initialized
            && handle.Index >= 0
            && handle.Index < timers.Length
            && versions[handle.Index] == handle.Version;
    }

    /// <summary>
    /// Called every frame by PlayerLoop. Runs Burst job then dispatches callbacks.
    /// </summary>
    public static void UpdateTimers()
    {
        if (!initialized || timers.Length == 0) return;

        float dt = Time.deltaTime;

        new TickTimersJob
        {
            Timers = timers.AsArray(),
            DeltaTime = dt
        }.Schedule(timers.Length, 32).Complete();

        // Dispatch managed callbacks on main thread
        for (int i = 0; i < timers.Length; i++)
        {
            ref var t = ref timers.ElementAt(i);

            if (t.TickFired == 1)
            {
                t.TickFired = 0;
                onTick[i]?.Invoke();
            }

            if (t.FinishedFired == 1)
            {
                t.FinishedFired = 0;
                t.IsRunning = 0;
                onFinished[i]?.Invoke();
            }
        }
    }

    /// <summary>
    /// Releases all timers and clears all state. Call on play mode exit.
    /// </summary>
    public static void Clear()
    {
        if (!initialized) return;

        for (int i = 0; i < timers.Length; i++)
        {
            onTick[i] = null;
            onFinished[i] = null;
        }
        timers.Clear();
        versions.Clear();
        freeSlots.Clear();
    }

    /// <summary>
    /// Disposes all NativeContainers. Call on application quit or domain unload.
    /// </summary>
    public static void Dispose()
    {
        if (!initialized) return;

        Clear();

        if (timers.IsCreated) timers.Dispose();
        if (versions.IsCreated) versions.Dispose();
        if (freeSlots.IsCreated) freeSlots.Dispose();

        onTick = null;
        onFinished = null;
        initialized = false;
    }

    private static void GrowManagedArrays(int requiredIndex)
    {
        if (requiredIndex < capacity) return;
        capacity = Mathf.Max(capacity * 2, requiredIndex + 1);
        Array.Resize(ref onTick, capacity);
        Array.Resize(ref onFinished, capacity);
    }

    [BurstCompile]
    private struct TickTimersJob : IJobParallelFor
    {
        public NativeArray<TimerData> Timers;
        [ReadOnly] public float DeltaTime;

        public void Execute(int i)
        {
            var t = Timers[i];
            if (t.IsRunning == 0 || t.IsFinished == 1) return;

            t.Elapsed += DeltaTime;
            t.TickFired = 0;
            t.FinishedFired = 0;

            // Check tick boundary
            if (t.Elapsed >= t.NextInterval && t.NextInterval > 0)
            {
                t.TickFired = 1;
                // Advance interval (catch up to prevent backlog)
                while (t.Elapsed >= t.NextInterval)
                    t.NextInterval += t.Interval;
            }

            // Check duration
            if (t.Duration > 0f && t.Elapsed >= t.Duration)
            {
                t.IsFinished = 1;
                t.FinishedFired = 1;
            }

            Timers[i] = t;
        }
    }
}
