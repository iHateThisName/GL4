using UnityEngine;

[CreateAssetMenu(fileName = "NightEvent", menuName = "TeamSuperSimple/Night", order = 0)]
public class SO_NightEvent : ScriptableObject
{
    [SerializeField] private MonsterSpawnEventData[] monsterSpawnEvents;
    [SerializeField] private RadioEventData[] radioEvents;

    public NightEvent[] GetEventData()
    {
        int total = (monsterSpawnEvents?.Length ?? 0) + (radioEvents?.Length ?? 0);
        var result = new NightEvent[total];
        int idx = 0;
        if (monsterSpawnEvents != null)
            foreach (var e in monsterSpawnEvents) result[idx++] = e.ToNightEvent();
        if (radioEvents != null)
            foreach (var e in radioEvents) result[idx++] = e.ToNightEvent();
        return result;
    }
}
