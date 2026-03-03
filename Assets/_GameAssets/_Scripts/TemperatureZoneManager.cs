using Assets.Scripts.Singleton;
using System.Collections.Generic;
using UnityEngine;

public class TemperatureZoneManager : Singleton<TemperatureZoneManager> {

    private readonly HashSet<PlayerTemperatureSimulator.EnumLocationType> activeZones = new HashSet<PlayerTemperatureSimulator.EnumLocationType>();

    public void EnterZone(PlayerTemperatureSimulator.EnumLocationType type) {
        Debug.Log("Player entered a new temperature zone: " + type);
        activeZones.Add(type);
        RecalculateZone();
    }

    public void ExitZone(PlayerTemperatureSimulator.EnumLocationType type) {
        Debug.Log("Player exited a temperature zone: " + type);
        activeZones.Remove(type);
        RecalculateZone();
    }

    /// <summary>
    /// Evaluates the player's active temperature zones by priorty and updates the current location type accordingly.
    /// </summary>
    /// <remarks>This method prioritizes the warm zone over shack, normal, and cold zones. If multiple zones
    /// are active, warm takes precedence unless cold is also present. If no recognized zone is active, the location
    /// type defaults to cold.</remarks>
    private void RecalculateZone() {
        if (activeZones.Contains(PlayerTemperatureSimulator.EnumLocationType.Warm) && !activeZones.Contains(PlayerTemperatureSimulator.EnumLocationType.Cold)) {
            PlayerTemperatureSimulator.Instance.SetLocationType(PlayerTemperatureSimulator.EnumLocationType.Warm);
        } else if (activeZones.Contains(PlayerTemperatureSimulator.EnumLocationType.Shack)) {
            PlayerTemperatureSimulator.Instance.SetLocationType(PlayerTemperatureSimulator.EnumLocationType.Shack);
        } else if (activeZones.Contains(PlayerTemperatureSimulator.EnumLocationType.Normal)) {
            PlayerTemperatureSimulator.Instance.SetLocationType(PlayerTemperatureSimulator.EnumLocationType.Normal);
        } else {
            PlayerTemperatureSimulator.Instance.SetLocationType(PlayerTemperatureSimulator.EnumLocationType.Cold);
        }
    }

    private void UpdateTemperatureZone(PlayerTemperatureSimulator.EnumLocationType type) {
        switch (type) {
            case PlayerTemperatureSimulator.EnumLocationType.Cold:
                Debug.Log("Player entered COLD temperature zone.");

                // TODO Apply cold vision

                // Remove heating vision
                //GameManager.Instance.FireAdaptationController.RemoveVolume();
                break;
            case PlayerTemperatureSimulator.EnumLocationType.Warm:
                Debug.Log("Player entered WARM temperature zone.");

                // Apply heating vision
                //GameManager.Instance.FireAdaptationController.ApplyVolume();
                break;
            case PlayerTemperatureSimulator.EnumLocationType.Normal:
                Debug.Log("Player entered NORMAL temperature zone.");

                // Remove heating vision
                //GameManager.Instance.FireAdaptationController.RemoveVolume();
                break;
            default:
                Debug.Log($"Player entered {type}");
                break;
        }

        //PlayerTemperatureSimulator.Instance.SetLocationType(type);
    }
}

