using UnityEngine;

/// <summary>
/// Per-event firing time within the night, expressed as a normalized 0-1 fraction.
/// 0 = start of night, 1 = end. Toggle <see cref="useRange"/> to pick a random value
/// between <see cref="timeMin"/> and <see cref="timeMax"/> instead of the fixed
/// <see cref="time"/>. Two events with the same resolved value fire simultaneously.
/// </summary>
[System.Serializable]
public struct NightEventTiming
{
    [Tooltip("If true, pick a random value between timeMin and timeMax. If false, use the fixed 'time' value.")]
    [SerializeField] private bool useRange;

    [Tooltip("Fixed firing time as a fraction of the night (0 = start, 1 = end).")]
    [Range(0f, 1f)]
    [SerializeField] private float time;

    [Tooltip("Earliest firing time as a fraction of the night.")]
    [Range(0f, 1f)]
    [SerializeField] private float timeMin;

    [Tooltip("Latest firing time as a fraction of the night.")]
    [Range(0f, 1f)]
    [SerializeField] private float timeMax;

    public bool UseRange => this.useRange;
    public float Time => this.time;
    public float TimeMin => this.timeMin;
    public float TimeMax => this.timeMax;

    /// <summary>Resolves a concrete normalized firing time, sampling the range if enabled.</summary>
    public float ResolveNormalized()
    {
        if (!this.useRange) return Mathf.Clamp01(this.time);
        float lo = Mathf.Min(this.timeMin, this.timeMax);
        float hi = Mathf.Max(this.timeMin, this.timeMax);
        return Mathf.Clamp01(Random.Range(lo, hi));
    }
}

[System.Serializable]
public struct NightEvent
{
    [System.Serializable]
    public enum NightEventType
    {
        SpawnMonster,
        SpawnFood,
        DisruptRadio,
        RadioBroadcast
    }
    
    [SerializeField] private GameObject monster;
    [SerializeField] private NightEventTiming timing;
    [SerializeField] private Radio.RadioBroadcasts radioBroadcast;
    [SerializeField] private NightEventType eventType;
    [SerializeField] private int amount;

    public NightEvent(NightEventType eventType, GameObject monster, int amount, NightEventTiming timing, Radio.RadioBroadcasts radioBroadcast)
    {
        this.eventType = eventType;
        this.monster = monster;
        this.timing = timing;
        this.amount = amount;
        this.radioBroadcast = radioBroadcast;
    }
    
    public GameObject GetMonsterPrefab() => this.monster;
    public NightEventTiming GetTiming() => this.timing;
    public NightEventType GetEventType() => this.eventType;
    public Radio.RadioBroadcasts GetRadioBroadcast() => this.radioBroadcast;
    public int GetAmount() => this.amount;
}

[System.Serializable]
public struct MonsterSpawnEventData
{
    [SerializeField] private GameObject monster;
    [SerializeField] private int monsterCount;
    [SerializeField] private NightEventTiming timing;

    public NightEvent ToNightEvent() => new NightEvent(NightEvent.NightEventType.SpawnMonster, this.monster, Mathf.Max(1, this.monsterCount), this.timing, Radio.RadioBroadcasts.Off);
}

[System.Serializable]
public struct RadioEventData
{
    public enum RadioEventType { Disrupt, Broadcast }
    [SerializeField] private NightEventTiming timing;
    [SerializeField] private RadioEventType type;
    [SerializeField] private Radio.RadioBroadcasts radioBroadcast;
    [SerializeField] private int priority;

    public NightEvent ToNightEvent()
    {
        var nightEventType = type == RadioEventType.Broadcast
            ? NightEvent.NightEventType.RadioBroadcast
            : NightEvent.NightEventType.DisruptRadio;
        return new NightEvent(nightEventType, null, this.priority, this.timing, this.radioBroadcast);
    }
}
