using UnityEngine;

/// <summary>
/// Manages a dynamic heat zone around a fireplace that expands and contracts based on fuel levels.
/// The heat zone is represented by a sphere collider that resizes smoothly as the fireplace burns.
/// </summary>
[RequireComponent(typeof(SphereCollider))]
public class FireplaceHeatZone : MonoBehaviour {
    [Header("Refrences")]
    [SerializeField] private FireplaceController fireplace; // Reference to the fireplace controller
    [SerializeField] private SphereCollider heatCollider; // Sphere collider representing the heat zone

    [Header("Configuration")]
    [SerializeField] private float minRadius = 0.5f; // Minimum radius of the heat zone when fuel is empty, but still lit.
    [SerializeField] private float maxRadius = 6f; // Maximum radius of the heat zone when max heat fuel is reached.
    [SerializeField] private float resizeSpeed = 1.2f; // Speed at which the heat zone resizes.

    private float smoothFuelPercentage = 0f; // Smoothed fuel percentage for gradual resizing

    private void Awake() {
        // Seting up the heat collider
        this.heatCollider = GetComponent<SphereCollider>();
        this.heatCollider.isTrigger = true;
    }

    private void FixedUpdate() {
        if (!this.fireplace.IsLit) {
            // Fireplace is not lit, shrink the heat zone to nothing
            this.heatCollider.radius = Mathf.Lerp(this.heatCollider.radius, 0.1f, Time.fixedDeltaTime * this.resizeSpeed);
            return;
        }

        // Smooth the fuel percentage for gradual resizing
        this.smoothFuelPercentage = Mathf.Lerp(this.smoothFuelPercentage, this.fireplace.FuelPercentage, Time.fixedDeltaTime * this.resizeSpeed);

        // Calculate target radius based on fuel percentage
        float targetRadius = Mathf.Lerp(this.minRadius, this.maxRadius, this.smoothFuelPercentage);

        // Smoothly resize the heat zone towards the target radius
        this.heatCollider.radius = Mathf.Lerp(this.heatCollider.radius, targetRadius, Time.fixedDeltaTime * this.resizeSpeed);
    }
}
