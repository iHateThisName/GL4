using UnityEngine;

[CreateAssetMenu(fileName = "NightEvent", menuName = "TeamSuperSimple/Night", order = 0)]
public class SO_NightEvent : ScriptableObject
{
    [SerializeField] private GameManager.NightEventData[] eventData;
    
    public GameManager.NightEventData[] GetEventData() => this.eventData;
}