using System;
using UnityEngine;

public class FireplaceController : MonoBehaviour {

    [Header("Fuel Settings")]
    [SerializeField] private float currentFuel = 0f;
    private float burnRate = 0.5f;
    private const float MAX_FUEL = 100f;

    public bool IsLit { get; private set; } = false;

    private void FixedUpdate() {
        if (IsLit) {
            BurnFuel();
        }
    }

    private void BurnFuel() {

        // Burn fuel over time
        this.currentFuel -= this.burnRate * Time.fixedDeltaTime;

        // Check if fuel has run out
        if (this.currentFuel <= 0f) {
            //Extinguish Fire
            this.currentFuel = 0f;
            this.IsLit = false;
        }
    }

    [ContextMenu("Add Fuel (10)")]
    private void AddFuelDebug() {
        // For testing purposes
        AddFuel(10f);
    }

    public void AddFuel(float amount) {
        // Only allow adding fuel if the fireplace is lit??

        // Automatically light the fire if it was out when fuel is added for now.
        if (this.currentFuel == 0f && !this.IsLit) {
            this.IsLit = true;
        }

        // Allow adding fuel even if max is reached
        this.currentFuel = Mathf.Min(currentFuel + amount, MAX_FUEL);
    }
}
