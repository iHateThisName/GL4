using UnityEngine;

[CreateAssetMenu(fileName = "NightEvent", menuName = "TeamSuperSimple/Night", order = 0)]
public class SO_NightEvent : ScriptableObject
{
    [SerializeField] private NightEventData[] eventData;
    
    public NightEventData[] GetEventData() => this.eventData;
}