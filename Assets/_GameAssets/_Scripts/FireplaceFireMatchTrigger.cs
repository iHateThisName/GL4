using UnityEngine;

public class FireplaceFireMatchTrigger : MonoBehaviour {

    [SerializeField] private FireplaceController fireplaceController; // Direct reference to the fireplace controller to manage fuel and ignition state.

    /// <summary>
    /// Handles the trigger event when another collider enters the trigger zone. Initiates the fire ignition process
    /// when the entering collider is tagged as 'FireMatch'.
    /// </summary>
    /// <remarks>This method retrieves the FireMatchController component from the root of the entering collider and uses it to ignite the fireplace.</remarks>
    /// <param name="other">The collider that has entered the trigger zone. Must be tagged as 'FireMatch' to trigger the fire ignition
    /// process.</param>
    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("FireMatch")) return;

        Debug.Log("Fireplace fuel trigger detected collision with: " + other.gameObject.name);
        FireMatchController fireMatchController = other.transform.root.GetComponentInChildren<FireMatchController>();
        IgniteFireplace(fireMatchController);
    }

    /// <summary>
    /// Attempts to ignite the fireplace using the specified fire match controller, provided that fuel is present and
    /// the fireplace is not already lit.
    /// </summary>
    /// <remarks>If the fireplace lacks fuel or is already lit, the ignition will not proceed and debug
    /// messages will be logged to indicate the reason.</remarks>
    /// <param name="fireMatchController">The fire match controller used to initiate the ignition process for the fireplace.</param>
    private void IgniteFireplace(FireMatchController fireMatchController) {
        if (!this.fireplaceController.HasFuel) {
            Debug.Log("Cannot ignite fireplace: No fuel present.");
            return;
        }
        if (this.fireplaceController.IsLit) {
            Debug.Log("Fireplace is already lit.");
            return;
        }

        this.fireplaceController.Ignite(fireMatchController);

    }
}
