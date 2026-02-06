using TMPro;
using UnityEngine;

/// <summary>
/// Manages the player's hunger over time, handles eating food,
/// and exposes starvation state changes for other systems to react to.
/// </summary>
public class HungerSystem : MonoBehaviour
{
    public enum EnumHungerState
    {
        Full,
        SlightlyHungry,
        Hungry,
        Starving
    }

    [System.Serializable]
    public struct FHungerThreshold
    {
        public EnumHungerState state;
        public float key;
    }
    
    [SerializeField] private float hunger; 
    // Current hunger value. Higher means the player is well-fed, lower means starving.

    [SerializeField] private TriggerArea mouthCollider; 
    // Trigger area representing the player's mouth.
    // When food enters this collider, the player "eats" it.

    [SerializeField] private float maxHunger = 100f; 
    // Maximum hunger value the player can have.

    [SerializeField] private float hungerDecayRate = 0.01f; 
    // Amount of hunger lost each hunger tick.

    [SerializeField] private float hungerDecayTick = 0.25f; 
    // Time interval (seconds) between hunger decay updates.

    [SerializeField] private TextMeshProUGUI hungerText; 
    // UI text element displaying the current hunger value.
    // threshold lookup map for logic.
    
    private EnumHungerState hungerState;
    // current state of hunger
    
    private float elapsedTime;
    // Tracks time passed since the last hunger decay tick.

    private const float SLIGHTY_HUNGRY_THRESHOLD = 0.8f;
    private const float HUNGER_THRESHOLD = .5f;
    private const float STARVING_THRESHOLD = .25f;

    // Event fired whenever the starvation state changes.
    // The bool parameter is true when starving, false when no longer starving.
    public delegate void HungerStateChanged(EnumHungerState previous, EnumHungerState current);
    // Event signature for HungerStateChanged
    public static event HungerStateChanged HungerStateChangedEvent;

    /// <summary>
    /// Initializes the hunger system by setting hunger to maximum
    /// and updating the UI to reflect the starting value.
    /// </summary>
    private void Start()
    {
        this.hunger = this.maxHunger;
        this.hungerState = EnumHungerState.Full;
        this.UpdateHungerText(this.hungerText, this.hunger.ToString("F2"));
    }

    /// <summary>
    /// Subscribes to the mouth trigger event when enabled.
    /// Ensures the player can eat food when it enters the mouth collider.
    /// </summary>
    private void OnEnable()
    {
        if (this.mouthCollider == null) return;
        this.mouthCollider.OnTriggerEntered += this.tryEatFood;
    }

    /// <summary>
    /// Unsubscribes from the mouth trigger event when disabled.
    /// Prevents event leaks and duplicate subscriptions.
    /// </summary>
    private void OnDisable()
    {
        if (this.mouthCollider == null) return;
        this.mouthCollider.OnTriggerEntered -= this.tryEatFood;
    }

    private void tryEatFood(Collider other)
    {
        if (!other.TryGetComponent<Food>(out var food)) return;
        
        if (this.hunger.Equals(this.maxHunger)) return;
        
        eatFood(food);
    }

    /// <summary>
    /// Called when an object enters the mouth trigger.
    /// If the object is a Food item, the player consumes it and gains hunger.
    /// </summary>
    /// <param name="other">The collider that entered the mouth trigger.</param>
    private bool eatFood(Food food)
    {
        if (food == null) return false;
        
        // Increase hunger by the food's value, clamped to avoid going below zero
        this.hunger = Mathf.Max(this.hunger + food.GetFoodValue(), 0);

        // Update UI after eating
        this.UpdateHungerText(this.hungerText, this.hunger.ToString("F2"));

        // If hunger has risen above the threshold, clear starvation state if needed
        SetHungerState(GetHungerState(this.hunger));

        // Destroy the food object shortly after being eaten
        Destroy(food.gameObject, 0.1f);
        return true;
    }

    /// <summary>
    /// Handles hunger decay over time.
    /// Every hungerDecayTick seconds, hunger decreases by hungerDecayRate.
    /// Updates the UI and checks whether the player has reached or left starvation.
    /// </summary>
    private void Update()
    {
        this.elapsedTime += Time.deltaTime;

        // Only decay hunger once the tick interval has passed
        if (this.elapsedTime >= this.hungerDecayTick)
        {
            this.elapsedTime = 0;

            // Reduce hunger and clamp between 0 and maxHunger
            this.hunger = Mathf.Clamp(this.hunger - this.hungerDecayRate, 0, this.maxHunger);

            // Update UI text
            this.UpdateHungerText(this.hungerText, this.hunger.ToString("F2"));

            // Check if hunger has fallen below or risen above the starvation threshold
            SetHungerState(GetHungerState(this.hunger));
        }
    }

    private EnumHungerState GetHungerState(float currentHunger)
    {
        float slightHungryThreshold = SLIGHTY_HUNGRY_THRESHOLD * this.maxHunger;
        float hungerThreshold = HUNGER_THRESHOLD * this.maxHunger;
        float starvingThreshold = STARVING_THRESHOLD * this.maxHunger;
        
        if (currentHunger <= slightHungryThreshold && currentHunger > hungerThreshold) return EnumHungerState.SlightlyHungry;
        else if (currentHunger <= hungerThreshold && currentHunger > starvingThreshold) return EnumHungerState.Hungry;
        else if (currentHunger <= starvingThreshold && currentHunger > 0) return EnumHungerState.Starving;
        else return EnumHungerState.Full;
    }
    
    private void SetHungerState(EnumHungerState newHungerState)
    {
        if (this.hungerState == newHungerState) return;
        EnumHungerState previousState = this.hungerState;
        this.hungerState = newHungerState;
        HungerStateChangedEvent?.Invoke(previousState, this.hungerState);
    }

    /// <summary>
    /// Updates the hunger UI text field with a new value.
    /// </summary>
    /// <param name="textField">The UI text element to update.</param>
    /// <param name="newText">The new text to display.</param>
    private void UpdateHungerText(TextMeshProUGUI textField, string newText)
    {
        if (textField == null) return;
        textField.text = newText;
    }
}