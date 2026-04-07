/// <summary>
/// Lightweight value-type handle to a timer slot in TimerManager.
/// Version field prevents use-after-free when a slot is recycled.
/// </summary>
public readonly struct TimerHandle
{
    public readonly int Index;
    public readonly int Version;

    public TimerHandle(int index, int version)
    {
        Index = index;
        Version = version;
    }

    public bool IsValid => Index >= 0;

    public static readonly TimerHandle Invalid = new(-1, 0);
}
