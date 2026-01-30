using TMPro;
using UnityEngine;

/// <summary>
/// Manages the player's hunger over time, handles eating food,
/// and exposes starvation state changes for other systems to react to.
/// </summary>
public class HungerSystem : MonoBehaviour
{
    [SerializeField] private float hunger; 
    // Current hunger value. Higher means the player is well-fed, lower means starving.

    [SerializeField] private TriggerArea mouthCollider; 
    // Trigger area representing the player's mouth.
    // When food enters this collider, the player "eats" it.

    [SerializeField] private float maxHunger = 100f; 
    // Maximum hunger value the player can have.

    [SerializeField] private float hungerThreshold = 0.5f; 
    // Threshold below which the player is considered starving.

    [SerializeField] private float hungerDecayRate = 0.01f; 
    // Amount of hunger lost each decay tick.

    [SerializeField] private float hungerDecayTick = 0.25f; 
    // Time interval (seconds) between hunger decay updates.

    [SerializeField] private TextMeshProUGUI hungerText; 
    // UI text element displaying the current hunger value.

    // Event fired whenever the starvation state changes.
    // The bool parameter is true when starving, false when no longer starving.
    public static System.Action<bool> OnStarvationStateChanged;

    private float elapsedTime; 
    // Tracks time passed since the last hunger decay tick.

    private bool starving; 
    // Indicates whether the player is currently starving.

    /// <summary>
    /// Initializes the hunger system by setting hunger to maximum
    /// and updating the UI to reflect the starting value.
    /// </summary>
    private void Start()
    {
        this.hunger = this.maxHunger;
        this.starving = false;
        this.UpdateHungerText(this.hungerText, this.hunger.ToString("F2"));
    }

    /// <summary>
    /// Subscribes to the mouth trigger event when enabled.
    /// Ensures the player can eat food when it enters the mouth collider.
    /// </summary>
    private void OnEnable()
    {
        if (this.mouthCollider == null) return;
        this.mouthCollider.OnTriggerEntered += this.eatFood;
    }

    /// <summary>
    /// Unsubscribes from the mouth trigger event when disabled.
    /// Prevents event leaks and duplicate subscriptions.
    /// </summary>
    private void OnDisable()
    {
        if (this.mouthCollider == null) return;
        this.mouthCollider.OnTriggerEntered -= this.eatFood;
    }

    /// <summary>
    /// Called when an object enters the mouth trigger.
    /// If the object is a Food item, the player consumes it and gains hunger.
    /// </summary>
    /// <param name="other">The collider that entered the mouth trigger.</param>
    private void eatFood(Collider other)
    {
        Debug.Log("Trigger Enter with: " + other.name);

        // Check if the object is a Food item
        if (!other.TryGetComponent<Food>(out var food)) return;

        // Increase hunger by the food's value, clamped to avoid going below zero
        this.hunger = Mathf.Max(this.hunger + food.GetFoodValue(), 0);

        // Update UI after eating
        this.UpdateHungerText(this.hungerText, this.hunger.ToString("F2"));

        // If hunger has risen above the threshold, clear starvation state if needed
        if (this.starving && this.hunger > this.hungerThreshold)
        {
            SetStarving(false);
        }

        // Destroy the food object shortly after being eaten
        Destroy(food.gameObject, 0.1f);
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
            if (!this.starving && this.hunger <= this.hungerThreshold)
            {
                SetStarving(true);
            }
            else if (this.starving && this.hunger > this.hungerThreshold)
            {
                SetStarving(false);
            }
        }
    }

    /// <summary>
    /// Updates the starvation state and notifies listeners if it changed.
    /// </summary>
    /// <param name="isStarving">New starvation state.</param>
    private void SetStarving(bool isStarving)
    {
        if (this.starving == isStarving) return;

        this.starving = isStarving;
        OnStarvationStateChanged?.Invoke(this.starving);
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