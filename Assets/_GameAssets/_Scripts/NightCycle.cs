using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class NightCycle : MonoBehaviour
{
    [Tooltip("Total night duration (minutes)")]
    [SerializeField] private float timePerNightMinutes;
    [Tooltip("Minimum seconds before next event")]
    [SerializeField] private float minEventTimeSeconds;
    [Tooltip("Maximum seconds before next event")] 
    [SerializeField] private float maxEventTimeSeconds;

    public static event Action OnEventAvailable = delegate {};

    private float nightTimeInSeconds;
    private float elapsedNightTime;
    private float eventTime;

    private void OnEnable()
    {
        OnEventAvailable += DebugEventTimeWorking;
    }

    private void OnDisable()
    {
        OnEventAvailable -= DebugEventTimeWorking;
    }
    
    private void Start()
    {
        this.elapsedNightTime = 0;
        this.nightTimeInSeconds = this.timePerNightMinutes * 60;
        ScheduleNewEventTime();
    }

    private void Update()
    {
        if (this.elapsedNightTime >= this.nightTimeInSeconds) return;

        this.elapsedNightTime += Time.deltaTime;
        if (this.elapsedNightTime >= this.eventTime)
        {
            OnEventAvailable.Invoke();
            ScheduleNewEventTime();
        }
    }

    private void ScheduleNewEventTime()
    {
        float remainingTime = this.nightTimeInSeconds - this.elapsedNightTime;
        float min = this.elapsedNightTime + this.minEventTimeSeconds;
        float max = this.elapsedNightTime + this.maxEventTimeSeconds;

        float newEventTime = Random.Range(min, max);
        this.eventTime = Mathf.Min(newEventTime, this.nightTimeInSeconds + 5);
    }

    private void DebugEventTimeWorking()
    {
        Debug.Log($"Event fired at night time: {this.elapsedNightTime:F2}, next at: {this.eventTime:F2}");
    }
}
