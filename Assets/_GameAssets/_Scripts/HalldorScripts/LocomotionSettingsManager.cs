using UnityEngine;
using UnityEngine.InputSystem;

public class LocomotionSettingsManager : MonoBehaviour
{
    [Header("Teleport Input Actions")]
    [Tooltip("Drag the Right Teleport Activate action here")]
    public InputActionReference teleportActivateAction;

    [Tooltip("Drag the Right Teleport Cancel action here")]
    public InputActionReference teleportCancelAction;

    // This is the function your Main Menu Toggle will call
    public void SetTeleportEnabled(bool isEnabled)
    {
        if (teleportActivateAction != null && teleportActivateAction.action != null)
        {
            if(isEnabled)
            {
                teleportActivateAction.action.Enable();
            }
            else
            {
                teleportActivateAction.action.Disable();
            }
        }

        if (teleportCancelAction != null && teleportCancelAction.action != null)
        {
            if(isEnabled)
            {
                teleportCancelAction.action.Enable();
            }
            else
            {
                teleportCancelAction.action.Disable();
            }
        }
        Debug.Log("Teleportation enabled state set to: " + isEnabled);
    }
}
