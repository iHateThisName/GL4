using UnityEngine;

[System.Serializable]
public enum NightEventType
{
    SpawnMonster,
    SpawnFood,
    DisruptRadio,
    RadioBroadcast
}
    
[System.Serializable]
public struct NightEventData
{
    [SerializeField] private NightEventType eventType;
    [SerializeField] private GameObject monster;
    [SerializeField] private int monsterCount;
        
    public NightEventType GetEventType() => this.eventType;
    public GameObject GetMonsterPrefab() => this.monster;
    public int GetMonsterCount() => this.monsterCount;
}
    
[System.Serializable]
public struct NightEvent
{
    [SerializeField] private int eventIdx;
    [SerializeField] private int night;
    [SerializeField] private NightEventData eventData;
        
    public NightEvent(NightEventData eventData, int idx, int night) 
    {
        this.eventIdx = idx;
        this.night = night;
        this.eventData = eventData;
    }
        
    public int Index => this.eventIdx;

    public int Night => this.night;

    public NightEventData GetPayload() => this.eventData;
}