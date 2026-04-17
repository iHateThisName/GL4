using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class FireplaceController : MonoBehaviour {

    [Header("Fuel Settings")]
    private readonly float BURN_RATE = 0.5f;
    private readonly float MAX_FUEL_FOR_FULL_HEAT = 200f;
    private readonly Stack<Firewood> fuelQueue = new Stack<Firewood>(); // Stack to hold firewood fuel
    [SerializeField] private TutorialManager tutorialManager; //A refrence to the tutorial manager
    [field: SerializeField] public bool IsLit { get; private set; } = false; // Indicates if the fireplace is currently lit and will burn fuel.
    [field: SerializeField] public bool HasFuel { get; private set; } = false; // Indicates if there is any fuel left in the fireplace.

    [SerializeField] private Firewood currentBurningFuel; // The firewood currently being burned, null if none.
    private bool hasNewFuel = false; // Flag to indicate if new fuel has been added since last burn.
    private float cachedFuelAmount; // Cached total fuel to avoid iterating the stack every frame.

    /// <summary>
    /// Total remaining fuel across all firewood currently in the fireplace
    /// </summary>
    public float CurrentFuelAmount => cachedFuelAmount;

    /// <summary>
    /// Gets the current fuel level as a percentage (0-1) relative to the maximum fuel required for full heat output. Normalized value.
    /// </summary>

    [Header("Refrences")]
    public float FuelPercentage {
        get {
            return Mathf.Clamp01(this.CurrentFuelAmount / this.MAX_FUEL_FOR_FULL_HEAT);
        }
    }

    [SerializeField] private GameObject fireVFX;
    private void FixedUpdate() {

        //Audio logic
        HandleFireAudio();

        if (fuelQueue.Count > 0) {
            this.HasFuel = true;
            if (IsLit) {
                BurnFuel();
            }
        } else {
            this.HasFuel = false;
        }
    }

    [Header("Audio Settings")]
    [SerializeField] private AudioSource fireAudioSource;
    [SerializeField][Range(0f, 1f)] private float maxVolume = 1.0f;

    /// <summary>
    /// Burns fuel over time at the specified burn rate. 
    /// Automatically extinguishes the fire when fuel runs out.
    /// </summary>
    private void BurnFuel() {

        // Burn fuel over time
        float burnAmount = this.BURN_RATE * Time.fixedDeltaTime;

        // Check for fuel
        if (this.currentBurningFuel == null || this.hasNewFuel) {
            this.currentBurningFuel = fuelQueue.Peek(); // Get the last piece of firewood added
            this.hasNewFuel = false;
        }

        this.currentBurningFuel.RemainingFuel -= burnAmount; // Reduce its remaining fuel
        this.cachedFuelAmount -= burnAmount;

        // Check if fuel has run out
        if (this.currentBurningFuel.RemainingFuel <= 0) {
            float destroyedFuel = this.currentBurningFuel.RemainingFuel;

            // Destroy the fuel
            Destroy(this.currentBurningFuel.gameObject);
            this.currentBurningFuel = null;
            fuelQueue.Pop();

            if (this.fuelQueue.Count == 0) {
                // No more fuel left, extinguish fire
                this.cachedFuelAmount = 0f;
                this.HasFuel = false;
                this.IsLit = false;
                this.fireVFX.SetActive(false);
            } else if (destroyedFuel < 0) {
                // Apply remaining burn to next piece of firewood
                this.currentBurningFuel = fuelQueue.Peek();
                this.currentBurningFuel.RemainingFuel += destroyedFuel; // destroyedFuel is negative here
            }
        }
    }

    /// <summary>
    /// Debug method to add 10 fuel to the fireplace.
    /// Accessible via Unity's context menu in the inspector.
    /// </summary>
    [ContextMenu("Add Fuel (10)")]
    private void AddFuelDebug() {
        // For testing purposes
        ForceAddFuel(10f);
        Debug.Log($"Current Fuel Percentage: {this.FuelPercentage * 100f}%");
    }

    /// <summary>
    /// Debug method to add 100 fuel to the fireplace.
    /// Accessible via Unity's context menu in the inspector.
    /// </summary>
    [ContextMenu("Add Fuel (100)")]
    private void Add100FuelDebug() {
        // For testing purposes
        ForceAddFuel(100f);
        Debug.Log($"Current Fuel Percentage: {this.FuelPercentage * 100f}%");
    }

    /// <summary>
    /// Adds the specified amount of fuel to the fire and forces it to become lit, regardless of its previous state.
    /// </summary>
    /// <remarks>This method is intended for debugging or testing scenarios where fuel needs to be added
    /// directly, bypassing normal gameplay restrictions. Calling this method will always light the fire, even if it was
    /// previously extinguished.</remarks>
    /// <param name="amount">The amount of fuel to add to the fire, in fuel units. Must be a positive value.</param>
    public void ForceAddFuel(float amount) {

        // Create a dummy firewood object to represent the added fuel
        Firewood wood = new GameObject("DebugFirewood").AddComponent<Firewood>();
        wood.RemainingFuel = amount;
        wood.IsBurning = true;

        // Adding the fuel
        this.fuelQueue.Push(wood);
        this.cachedFuelAmount += amount;
        this.hasNewFuel = true;
        //this.IsLit = true; // Currently, adding fuel always lights the fire

        XRGrabInteractable grabInteractable = wood.GetComponent<XRGrabInteractable>();
        InteractionLayerMask mask = grabInteractable.interactionLayers;
        mask &= ~InteractionLayerMask.GetMask("Default"); // Remove default layer
        mask |= InteractionLayerMask.GetMask("Firewood"); // Making sure the Firewood mask is there
        grabInteractable.interactionLayers = mask;
    }

    /// <summary>
    /// Adds fuel to the fireplace.
    /// Automatically lights the fire if it was extinguished and fuel is added.
    /// </summary>
    /// <param name="amount">The amount of fuel to add.</param>
    public void AddFuel(Firewood wood) {
        // Ensure we dont add the same firewood multiple times
        if (wood.IsBurning) return;

        wood.IsBurning = true; // Mark the firewood as burning
        if (wood.RemainingFuel == 0) {
            wood.RemainingFuel = UnityEngine.Random.Range(wood.FuelValue * 0.8f, wood.FuelValue * 1.2f); // Randomize fuel value a bit for realism
        }
        // Adding the fuel
        this.fuelQueue.Push(wood);
        this.cachedFuelAmount += wood.RemainingFuel;
        this.hasNewFuel = true;
        //this.IsLit = true; // Currently, adding fuel always lights the fire

        // Making the firewood not interactable anymore since it's now part of the fire. All firewood should not be interactable when the fire is lit
        XRGrabInteractable grabInteractable = wood.GetComponent<XRGrabInteractable>();
        InteractionLayerMask mask = grabInteractable.interactionLayers;
        mask &= ~InteractionLayerMask.GetMask("Default"); // Remove default layer
        mask |= InteractionLayerMask.GetMask("Firewood"); // Making sure the Firewood mask is there
        grabInteractable.interactionLayers = mask; // Set the new interaction layers to the grab interactable.

        Debug.Log($"Current Fuel Percentage: {this.FuelPercentage * 100f}%");
    }

    public void Ignite(FireMatchController match) {
        if (tutorialManager != null && tutorialManager.hasLitFire == false)
        {
            tutorialManager.TurnOnFire();
        }
        this.IsLit = true;
        this.fireVFX.SetActive(true);
    }
    private void HandleFireAudio()
    {
        if (fireAudioSource == null) return;

        // Determine target volume: if not lit, it's 0. If lit, it's based on fuel.
        //0.1f minimum volume while still "Lit"
        float fuelPct = FuelPercentage;
        float targetVolume = IsLit ? Mathf.Max(fuelPct * maxVolume, 0.1f) : 0f;

        // Smoothly transition volume of fireplace
        fireAudioSource.volume = Mathf.MoveTowards(fireAudioSource.volume, targetVolume, Time.fixedDeltaTime * 0.5f);

        if (IsLit)
        {
            fireAudioSource.pitch = Mathf.Lerp(1.1f, 0.85f, fuelPct);
        }
    }
}
