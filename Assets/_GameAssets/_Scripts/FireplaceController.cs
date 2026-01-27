using System;
using UnityEngine;

public class FireplaceController : MonoBehaviour {

    [Header("Fuel Settings")]
    [SerializeField] private float currentFuel = 0f;
    private readonly float BURN_RATE = 0.5f;
    private readonly float MAX_FUEL = 100f;

    public bool IsLit { get; private set; } = false;

    private void FixedUpdate() {
        if (IsLit) {
            BurnFuel();
        }
    }

    /// <summary>
    /// Burns fuel over time at the specified burn rate. 
    /// Automatically extinguishes the fire when fuel runs out.
    /// </summary>
    private void BurnFuel() {

        // Burn fuel over time
        this.currentFuel -= this.BURN_RATE * Time.fixedDeltaTime;

        // Check if fuel has run out
        if (this.currentFuel <= 0f) {
            //Extinguish Fire
            this.currentFuel = 0f;
            this.IsLit = false;
        }
    }

    /// <summary>
    /// Debug method to add 10 fuel to the fireplace.
    /// Accessible via Unity's context menu in the inspector.
    /// </summary>
    [ContextMenu("Add Fuel (10)")]
    private void AddFuelDebug() {
        // For testing purposes
        AddFuel(10f);
    }

    /// <summary>
    /// Adds fuel to the fireplace, capped at MAX_FUEL.
    /// Automatically lights the fire if it was extinguished and fuel is added.
    /// </summary>
    /// <param name="amount">The amount of fuel to add.</param>
    public void AddFuel(float amount) {
        // Only allow adding fuel if the fireplace is lit??

        // Automatically light the fire if it was out when fuel is added for now.
        if (this.currentFuel == 0f && !this.IsLit) {
            this.IsLit = true;
        }

        // Allow adding fuel even if max is reached
        this.currentFuel = Mathf.Min(this.currentFuel + amount, this.MAX_FUEL);
    }
}
