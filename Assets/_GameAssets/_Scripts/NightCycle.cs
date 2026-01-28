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
        elapsedNightTime = 0;
        nightTimeInSeconds = timePerNightMinutes * 60;
        ScheduleNewEventTime();
    }

    private void Update()
    {
        if (elapsedNightTime >= nightTimeInSeconds) return;

        elapsedNightTime += Time.deltaTime;
        if (elapsedNightTime >= eventTime)
        {
            OnEventAvailable.Invoke();
            ScheduleNewEventTime();
        }
    }

    private void ScheduleNewEventTime()
    {
        float remainingTime = nightTimeInSeconds - elapsedNightTime;
        float min = elapsedNightTime + minEventTimeSeconds;
        float max = elapsedNightTime + maxEventTimeSeconds;

        float newEventTime = Random.Range(min, max);
        eventTime = Mathf.Min(newEventTime, nightTimeInSeconds + 5);
    }

    private void DebugEventTimeWorking()
    {
        Debug.Log($"Event fired at night time: {elapsedNightTime:F2}, next at: {eventTime:F2}");
    }
}
