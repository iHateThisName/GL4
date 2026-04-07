using UnityEngine;

public class TemperatureZoneTrigger : MonoBehaviour {

    [SerializeField] private PlayerTemperatureSimulator.EnumLocationType locationType; // The type of temperature zone this trigger represents.

    /// <summary>
    /// Handles the event when a collider enters the trigger zone, and initiates temperature zone transitions for the player.
    /// </summary>
    /// <param name="other">The collider that has entered the trigger zone. Must be tagged as 'Player' to trigger temperature zone changes.</param>
    private void OnTriggerEnter(Collider other) {
        if (!other.CompareTag("Player")) return; // Check if the entering collider is tagged as 'Player'. If not, exit the method.
        TemperatureZoneManager.Instance.EnterZone(this.locationType);

        // If the player enters a 'Normal' zone, ensure they exit the 'Cold' zone to maintain accurate temperature state since there is no trigger for cold zones
        if (this.locationType == PlayerTemperatureSimulator.EnumLocationType.Normal) {
            TemperatureZoneManager.Instance.ExitZone(PlayerTemperatureSimulator.EnumLocationType.Cold);
        }

    }

    /// <summary>
    /// Handles the event when a collider exits the trigger zone. If the exiting collider is tagged as 'Player', updates
    /// the temperature zone manager to reflect the player's departure from the current zone.
    /// </summary>
    /// <param name="other">The collider that exited the trigger zone. Must represent a player object, identified by the 'Player' tag.</param>
    private void OnTriggerExit(Collider other) {
        if (!other.CompareTag("Player")) return; // Check if the exiting collider is tagged as 'Player'. If not, exit the method.
        TemperatureZoneManager.Instance.ExitZone(this.locationType);

        // If the player exits a 'Normal' zone, ensure they enter the 'Cold' zone to maintain accurate temperature state since there is no trigger for cold zones
        if (this.locationType == PlayerTemperatureSimulator.EnumLocationType.Normal) {
            TemperatureZoneManager.Instance.EnterZone(PlayerTemperatureSimulator.EnumLocationType.Cold);
        }
    }


}
