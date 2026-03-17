using UnityEngine;

[CreateAssetMenu(fileName = "HungerSettings", menuName = "TeamSuperSimple/Hunger Settings", order = 0)]
public class SO_HungerSettings : ScriptableObject
{
    // Maximum hunger value the player can have.
    [SerializeField] private float maxHunger = 100f; 

    // Amount of hunger lost each hunger tick.
    [SerializeField] private float hungerDecayRate = 0.01f; 

    // Time interval (seconds) between hunger decay updates.
    [SerializeField] private float hungerDecayTick = 0.25f; 
    
    [SerializeField] private float foodValue = 20f;
    
    public float GetMaxHunger() => this.maxHunger;
    
    public float GetHungerDecayRate() => this.hungerDecayRate;
    
    public float GetHungerDecayTick() => this.hungerDecayTick;

    public float GetFoodFillValue() => this.foodValue;
}