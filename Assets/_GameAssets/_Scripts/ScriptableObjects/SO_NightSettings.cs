using UnityEngine;

[CreateAssetMenu(fileName = "NightSettings", menuName = "TeamSuperSimple/Night Settings", order = 0)]
public class SO_NightSettings : ScriptableObject
{
    // Total duration of the night in minutes. Converted to seconds internally.
    [Tooltip("Total night duration (minutes)")]
    public float nightTimeMinutes = 8;

    [SerializeField] private SO_NightEvent[] nightEvents;

    [Header("Debug")]
    [SerializeField] private int debugStartNight = 1;

    public int GetFinalNight() => this.nightEvents.Length;

    public int DebugStartNight => this.debugStartNight;
    public void SetDebugStartNight(int night) => this.debugStartNight = Mathf.Max(1, night);

    public float GetNightTimeInSeconds() => this.nightTimeMinutes * 60;

    /// <summary>
    /// Resolves each event's per-event timing into absolute seconds-from-night-start
    /// and returns the schedule sorted ascending. Events with identical resolved times
    /// will fire on the same tick (preserving their declared order as a tiebreaker).
    /// </summary>
    public void ForEachEventAcrossAllNights(System.Action<NightEventData> callback)
    {
        for (int n = 0; n < this.nightEvents.Length; n++)
        {
            if (this.nightEvents[n] == null) continue;
            NightEventData[] events = this.nightEvents[n].GetEventData();
            for (int i = 0; i < events.Length; i++)
                callback(events[i]);
        }
    }

    public ScheduledNightEvent[] BuildScheduleForNight(int night)
    {
        int nightIndex = night - 1;
        NightEventData[] events = nightIndex >= this.nightEvents.Length ? 
            new NightEventData[0] 
            : this.nightEvents[nightIndex].GetEventData();
        
        var schedule = new ScheduledNightEvent[events.Length];
        float nightSeconds = GetNightTimeInSeconds();

        for (int i = 0; i < events.Length; i++)
        {
            float normalized = events[i].GetTiming().ResolveNormalized();
            float seconds = Mathf.Clamp(normalized * nightSeconds, 0f, nightSeconds);
            schedule[i] = new ScheduledNightEvent(events[i], seconds, i);
        }

        // Stable sort: ascending by time, then by original index for tiebreaking.
        System.Array.Sort(schedule, (a, b) =>
        {
            int t = a.TimeSeconds.CompareTo(b.TimeSeconds);
            return t != 0 ? t : a.OriginalIndex.CompareTo(b.OriginalIndex);
        });
        return schedule;
    }
}

/// <summary>
/// A NightEventData paired with its resolved absolute firing time within a single night.
/// </summary>
public readonly struct ScheduledNightEvent
{
    public readonly NightEventData Data;
    public readonly float TimeSeconds;
    public readonly int OriginalIndex;

    public ScheduledNightEvent(NightEventData data, float timeSeconds, int originalIndex)
    {
        this.Data = data;
        this.TimeSeconds = timeSeconds;
        this.OriginalIndex = originalIndex;
    }
}