/// <summary>
/// Blittable timer data stored in a flat NativeArray. No heap allocation.
/// Burst-compatible for parallel tick accumulation.
/// </summary>
public struct TimerData
{
    public float Elapsed;
    public float Interval;
    public float Duration;      // 0 = infinite (runs until manually stopped)
    public float NextInterval;
    public byte IsRunning;      // 1 = running, 0 = paused/stopped
    public byte IsFinished;
    public byte TickFired;      // Set to 1 by Burst job when a tick boundary was crossed
    public byte FinishedFired;  // Set to 1 by Burst job when duration was reached
}
