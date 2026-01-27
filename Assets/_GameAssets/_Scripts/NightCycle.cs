using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class NightCycle : MonoBehaviour
{
    [Tooltip("Total night duration (minutes)")]
    [SerializeField] private float timePerNightMinutes;
    [Tooltip("Minimum seconds before next event")]
    [SerializeField] private float minEventTimeSeconds;
    
    
    [Range(1,20)] [Tooltip("Maximum seconds before next event")] [SerializeField] private float maxEventTimeSeconds;

    public static event Action OnEventAvailable = delegate {};

    private float _nightTimeInSeconds;
    private float _elapsedNightTime;
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
        _elapsedNightTime = 0;
        _nightTimeInSeconds = timePerNightMinutes * 60;
        ScheduleNewEventTime();
    }

    private void Update()
    {
        if (_elapsedNightTime >= _nightTimeInSeconds) return;

        _elapsedNightTime += Time.deltaTime;
        if (_elapsedNightTime >= eventTime)
        {
            OnEventAvailable.Invoke();
            ScheduleNewEventTime();
        }
    }

    private void ScheduleNewEventTime()
    {
        float remainingTime = _nightTimeInSeconds - _elapsedNightTime;
        float min = _elapsedNightTime + minEventTimeSeconds;
        float max = _elapsedNightTime + maxEventTimeSeconds;

        float newEventTime = Random.Range(min, max);
        eventTime = Mathf.Min(newEventTime, _nightTimeInSeconds + 5);
    }

    private void DebugEventTimeWorking()
    {
        Debug.Log($"Event fired at night time: {_elapsedNightTime:F2}, next at: {eventTime:F2}");
    }
}
