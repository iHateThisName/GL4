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
    //SFX of eating food
    [SerializeField] private AudioClip eatSFX;

    [Header("=== Configuration ====")]
    [SerializeField] private SO_HungerSettings hungerSettings;

    private TimerHandle timerHandle;
    
    // current state of hunger
    private EnumHungerState hungerState;
    
    // Internal current hunger value. Higher means the player is well-fed, lower means starving.
    [SerializeField] private float hunger;

    private const float SLIGHTY_HUNGRY_THRESHOLD = 0.8f;
    private const float HUNGER_THRESHOLD = 0.5f;
    private const float STARVING_THRESHOLD = 0.25f;

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
    private void Awake()
    {
        if (this.hungerSettings != null)
        {
            this.hunger = this.MaxHunger;
            this.hungerState = EnumHungerState.Full;

            this.timerHandle = TimerManager.Create(this.hungerSettings.GetHungerDecayTick());
            TimerManager.SetCallbacks(this.timerHandle, HandleHungerTick, null);
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
    
    private void OnDestroy()
    {
        TimerManager.Release(ref this.timerHandle);
    }

    /// <summary>
    /// Called when an object enters the mouth trigger.
    /// Tries to see if the collided item is a food item
    /// If the object is a Food item, the player consumes it and gains hunger.
    /// </summary>
    /// <param name="other">The collider that entered the mouth trigger.</param>
    private void tryEatFood(Collider other)
    {
        var foodObject = TryGetFood(other);
        if (foodObject == null) return;
        if (foodObject.Value <= 0)
        {
            Debug.Log("That is emptied");
            return;
        }

        if (this.hunger + this.hungerSettings.GetFoodFillValue() >= this.MaxHunger)
        {
            Debug.LogError("You are already full!");
            return;
        }
        
        eatFood(foodObject);
    }

    /// <summary>
    /// Processes the consumption of a food item.
    /// adding saturation to the player's hunger. and checks for state changes'
    /// </summary>
    /// <param name="food">food being processed.</param>
    private void eatFood(Food foodObject)
    {
        if (foodObject == null || foodObject.Value <= 0) return;
        
        foodObject.Eat();

        float foodDelta = foodObject.FillValue.Equals(-1)
            ? this.hungerSettings.GetFoodFillValue()
            : foodObject.FillValue;
        
        ClampHunger(foodDelta); // Increase hunger by the food's value

        //Play eatSFX
        if (eatSFX != null)
        {
            SoundEffectManager.Instance.PlaySoundFXClip(this.eatSFX, transform, 1f);
        }
    }

    /// <summary>
    /// Every hungerDecayTick seconds, hunger decreases by hungerDecayRate.
    /// </summary>
    private void HandleHungerTick()
    {
        if (this.hungerSettings == null) return;
        
        ClampHunger(-this.hungerSettings.GetHungerDecayRate());
            
        if (this.hunger <= 0)
        {
            Debug.Log("You are starving!");
            TimerManager.Release(ref this.timerHandle);
            DeathSystem.KillPlayer(DeathSystem.DeathEvent.DeathReason.Hunger, "", false);
        }
    }

    public void ModifyHunger(float delta)
    {
        ClampHunger(delta);
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
    
    public float Hunger => this.hunger;
    
    public EnumHungerState State => this.hungerState;
    
    public static Food TryGetFood(Collider other)
    {
        var foodObject = other.GetComponentInParent<Food>();
        if (foodObject == null)
        {
            Debug.LogWarning("That is not food!");
            return null;
        };
        return foodObject;
    }

    public void Pause()
    {
        if (!this.timerHandle.IsValid) return;
        TimerManager.Pause(this.timerHandle);
    }
}