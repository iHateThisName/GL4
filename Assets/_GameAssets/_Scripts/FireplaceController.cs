using System;
using System.Collections.Generic;
using UnityEngine;

public class FireplaceController : MonoBehaviour {

    [Header("Fuel Settings")]
    private readonly float BURN_RATE = 0.5f;
    // private readonly float MAX_FUEL = 100f; There is not need to cap because it will be limited by amount of nodes.

    private readonly Stack<Firewood> fuelQueue = new Stack<Firewood>();
    [field: SerializeField] public bool IsLit { get; private set; } = false;
    [field: SerializeField] public bool HasFuel { get; private set; } = false;

    [SerializeField] private Firewood currentBurningFuel;
    private bool hasNewFuel = false;

    /// <summary>
    /// Total remaining fuel across all firewood currently in the fireplace
    /// </summary>
    public float CurrentFuelAmount {
        get {
            float total = 0f;
            foreach (Firewood wood in fuelQueue) {
                total += Mathf.Max(wood.RemainingFuel, 0f);
            }
            return total;
        }
    }

    private void FixedUpdate() {
        if (IsLit && fuelQueue.Count > 0) {
            this.HasFuel = true;
            BurnFuel();
        }
    }

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

        // Check if fuel has run out
        if (this.currentBurningFuel.RemainingFuel <= 0) {
            float destroyedFuel = this.currentBurningFuel.RemainingFuel;

            // Destroy the fuel
            Destroy(this.currentBurningFuel.gameObject);
            this.currentBurningFuel = null;
            fuelQueue.Pop();

            if (this.fuelQueue.Count == 0) {
                // No more fuel left, extinguish fire
                this.HasFuel = false;
                this.IsLit = false;
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
    }


    public void ForceAddFuel(float amount) {

        // Create a dummy firewood object to represent the added fuel
        Firewood wood = new GameObject("DebugFirewood").AddComponent<Firewood>();
        wood.RemainingFuel = amount;
        wood.IsBurning = true;

        // Enqueue the firewood
        this.fuelQueue.Push(wood);
        this.hasNewFuel = true;
        this.IsLit = true; // Currently, adding fuel always lights the fire
    }

    /// <summary>
    /// Adds fuel to the fireplace, capped at MAX_FUEL.
    /// Automatically lights the fire if it was extinguished and fuel is added.
    /// </summary>
    /// <param name="amount">The amount of fuel to add.</param>
    public void AddFuel(Firewood wood) {
        // Ensure we dont add the same firewood multiple times
        if (wood.IsBurning) return;

        wood.IsBurning = true;
        wood.RemainingFuel = UnityEngine.Random.Range(wood.FuelValue * 0.8f, wood.FuelValue * 1.2f);

        // Enqueue the firewood
        this.fuelQueue.Push(wood);
        this.hasNewFuel = true;
        this.IsLit = true; // Currently, adding fuel always lights the fire
    }
}
