using UnityEngine;

/// <summary>
/// Controls the progression of a night cycle and triggers timed events during the night.
/// Events occur at randomized intervals within a configurable range.
/// </summary>
public class NightCycle : MonoBehaviour
{
    [Tooltip("Total night duration (minutes)")]
    [SerializeField] private float timePerNightMinutes; 
    // Total duration of the night in minutes. Converted to seconds internally.

    [Tooltip("Minimum seconds before next event")]
    [SerializeField] private float minEventTimeSeconds; 
    // Minimum delay (in seconds) before the next event can occur.

    [Tooltip("Maximum seconds before next event")]
    [SerializeField] private float maxEventTimeSeconds; 
    // Maximum delay (in seconds) before the next event can occur.

    // Event invoked whenever a scheduled night event becomes available.
    // Other systems can subscribe to react (e.g., spawning enemies, triggering sounds).
    public static event System.Action OnEventAvailable = delegate { };

    private float nightTimeInSeconds;   // Total night duration converted to seconds
    private float elapsedNightTime;     // Time passed since the night started
    private float eventTime;            // Timestamp (in seconds) when the next event should occur

    /// <summary>
    /// Unity callback invoked when the object becomes enabled.
    /// Subscribes a debug method to the event for testing purposes.
    /// </summary>
    private void OnEnable()
    {
        OnEventAvailable += DebugEventTimeWorking;
    }

    /// <summary>
    /// Unity callback invoked when the object becomes disabled.
    /// Unsubscribes the debug method to avoid memory leaks or duplicate logs.
    /// </summary>
    private void OnDisable()
    {
        OnEventAvailable -= DebugEventTimeWorking;
    }

    /// <summary>
    /// Initializes the night cycle by resetting timers and scheduling the first event.
    /// </summary>
    private void Start()
    {
        this.elapsedNightTime = 0f;
        this.nightTimeInSeconds = this.timePerNightMinutes * 60f; // Convert minutes to seconds
        ScheduleNewEventTime(); // Determine when the first event will occur
    }

    /// <summary>
    /// Updates the night timer and checks whether an event should fire.
    /// Stops updating once the night duration has been reached.
    /// </summary>
    private void Update()
    {
        // If the night has ended, no further updates or events should occur.
        if (this.elapsedNightTime >= this.nightTimeInSeconds) return;

        // Advance the night timer
        this.elapsedNightTime += Time.deltaTime;

        // Check if the next event time has been reached
        if (this.elapsedNightTime >= this.eventTime)
        {
            OnEventAvailable.Invoke(); // Notify subscribers
            ScheduleNewEventTime();    // Schedule the next event
        }
    }

    /// <summary>
    /// Schedules the next event by selecting a random time window within the allowed range.
    /// Ensures the event does not exceed the total night duration (with a small buffer).
    /// </summary>
    private void ScheduleNewEventTime()
    {
        float remainingTime = this.nightTimeInSeconds - this.elapsedNightTime;

        // Calculate the earliest and latest possible timestamps for the next event
        float min = this.elapsedNightTime + this.minEventTimeSeconds;
        float max = this.elapsedNightTime + this.maxEventTimeSeconds;

        // Pick a random timestamp between the min and max bounds
        float newEventTime = Random.Range(min, max);

        // Clamp event time so it never exceeds the night duration by more than a small buffer
        this.eventTime = Mathf.Min(newEventTime, this.nightTimeInSeconds + 5f);
    }

    /// <summary>
    /// Debug helper method that logs when an event fires and when the next one is scheduled.
    /// Useful for verifying timing behavior during development.
    /// </summary>
    private void DebugEventTimeWorking()
    {
        Debug.Log($"Event fired at night time: {this.elapsedNightTime:F2}, next at: {this.eventTime:F2}");
    }
}