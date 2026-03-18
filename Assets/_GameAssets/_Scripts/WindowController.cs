using System;
using UnityEngine;

public class WindowController : MonoBehaviour
{
    [SerializeField] private VRLever windowJoint;
    public Transform targetPosition; // The position the intruder will go

    private void OnEnable() {
        windowJoint.OnLeverStateChanged += HandleWindowStateChanged;
    }

    private void OnDisable() {
        windowJoint.OnLeverStateChanged -= HandleWindowStateChanged;
    }
    private void HandleWindowStateChanged(VRLever.EnumLeverState state) {
        GameManager.Instance.UpdateWindowState(this, state);
        Debug.Log($"Window state changed to {state}");
    }


    public Action<VRLever.EnumLeverState> GetWindowEvent() => windowJoint.OnLeverStateChanged;
    public VRLever.EnumLeverState GetCurrentWindowState() => windowJoint.CurrentState;
}
