using UnityEngine;

[CreateAssetMenu(fileName = "NightSettings", menuName = "TeamSuperSimple/Night Settings", order = 0)]
public class SO_NightSettings : ScriptableObject
{
    // Total duration of the night in minutes. Converted to seconds internally.
    [Tooltip("Total night duration (minutes)")]
    public float nightTimeMinutes = 8;
    
    [SerializeField] private int maxNights = 3;
    
    // Minimum delay (in seconds) before the next event can occur.
    [Tooltip("Minimum seconds before next event")]
    public float nightEventMinTimeSeconds = 570;
    
    // Maximum delay (in seconds) before the next event can occur.
    [Tooltip("Maximum seconds before next event")]
    public float nightEventMaxTimeSeconds = 570;
    
    [SerializeField] private SO_NightEvent[] nightEvents;
    
    public int GetFinalNight() => this.maxNights;
    
    public float GetNightTimeInSeconds() => this.nightTimeMinutes * 60;

    /// <summary>
    /// Schedules the next event by selecting a random time window within the allowed range.
    /// Ensures the event does not exceed the total night duration (with a small buffer).
    /// </summary>
    public float GetNewNightEventTime()
    {
        // Pick a random timestamp between the min and max bounds
        float newEventTime = Random.Range(this.nightEventMinTimeSeconds, this.nightEventMaxTimeSeconds);

        // Clamp event time so it never exceeds the night duration by more than a small buffer
        return Mathf.Min(newEventTime, GetNightTimeInSeconds() + 5f);
    }

    public NightEventData[] GetEventsForNight(int night)
    {
        return this.nightEvents[night - 1].GetEventData();
    }
}