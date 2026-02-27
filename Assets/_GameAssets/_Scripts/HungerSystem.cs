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

    [Header("=== References ====")]
    // Trigger area representing the player's mouth.
    // When food enters this collider, the player "eats" it.
    [SerializeField] private TriggerArea mouthCollider; 

    [Header("=== Configuration ====")]
    [SerializeField] private HungerSettings hungerSettings;
    
    private Timer hungerTimer;
    
    // current state of hunger
    private EnumHungerState hungerState;
    
    // Internal current hunger value. Higher means the player is well-fed, lower means starving.
    [SerializeField] private float hunger;
    
    // Tracks time passed since the last hunger decay tick.
    private float elapsedTime;

    private const float SLIGHTY_HUNGRY_THRESHOLD = 0.8f;
    private const float HUNGER_THRESHOLD = .5f;
    private const float STARVING_THRESHOLD = .25f;

    // Event fired whenever the starvation state changes.
    // The bool parameter is true when starving, false when no longer starving.
    public delegate void HungerStateChanged(EnumHungerState previous, EnumHungerState current);
    // Event signature for HungerStateChanged
    public static event HungerStateChanged HungerStateChangedEvent;
    
    public static System.Action<float> OnHungerChanged;
    
    public float MaxHunger => this.hungerSettings != null ? this.hungerSettings.GetMaxHunger() : 100;

    /// <summary>
    /// Initializes the hunger system by setting hunger to maximum
    /// and updating the UI to reflect the starting value.
    /// </summary>
    private void Start()
    {
        if (this.hungerSettings != null)
        {
            this.hunger = this.MaxHunger;
            this.hungerState = EnumHungerState.Full;
            
            // setup internal timer
            this.hungerTimer = new Timer(this.hungerSettings.GetHungerDecayTick(), 0);
            this.hungerTimer.OnTimerTick += HandleHungerTick;
            this.hungerTimer.Start();
        }
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
    
    /// <summary>
    /// Clean up the timer when this component is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        if (this.hungerTimer != null)
        {
            this.hungerTimer.Dispose();
            this.hungerTimer = null;
        }
    }

    /// <summary>
    /// Called when an object enters the mouth trigger.
    /// Tries to see if the collided item is a food item
    /// If the object is a Food item, the player consumes it and gains hunger.
    /// </summary>
    /// <param name="other">The collider that entered the mouth trigger.</param>
    private void tryEatFood(Collider other)
    {
        if (!other.transform.parent.TryGetComponent<Food>(out var food))
        {
            Debug.LogError("Food object entered mouth trigger but could not be cast to Food component.");
            return;
        }
        
        if (this.hunger.Equals(this.MaxHunger - 20)) return;
        
        eatFood(food);
    }

    /// <summary>
    /// Processes the consumption of a food item.
    /// adding saturation to the player's hunger. and checks for state changes'
    /// </summary>
    /// <param name="food">food being processed.</param>
    private bool eatFood(Food food)
    {
        if (food == null) return false;
        
        // Increase hunger by the food's value
        ClampHunger( food.GetFoodValue());

        // Destroy the food object shortly after being eaten
        Destroy(food.gameObject, 0.1f);
        return true;
    }

    /// <summary>
    /// Every hungerDecayTick seconds, hunger decreases by hungerDecayRate.
    /// </summary>
    private void HandleHungerTick()
    {
        if (this.hungerSettings == null) return;
        
        ClampHunger(-this.hungerSettings.GetHungerDecayRate());
            
        // Kill player if starved
        if (this.hunger <= 0)
        {
            Debug.Log("You are starving!");
            this.hungerTimer.Dispose();
            DeathSystem.KillPlayer(DeathSystem.DeathEvent.DeathReason.Hunger, false);
        }
    }

    private void ClampHunger(float delta)
    {
        // clamp the new hunger plus delta between 0 and maxHunger
        this.hunger = Mathf.Clamp(this.hunger + delta, 0, this.MaxHunger);
        OnHungerChanged?.Invoke(this.hunger); // Send event for listeners to update based on hunger
        SetHungerState(GetHungerState(this.hunger));
    }

    private EnumHungerState GetHungerState(float currentHunger)
    {
        float slightHungryThreshold = SLIGHTY_HUNGRY_THRESHOLD * this.MaxHunger;
        float hungerThreshold = HUNGER_THRESHOLD * this.MaxHunger;
        float starvingThreshold = STARVING_THRESHOLD * this.MaxHunger;
        
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
    
    #region DEPRECATED_HELPER
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
    #endregion
}