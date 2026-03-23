using System;
using UnityEngine;

public class WindowController : MonoBehaviour
{
    [SerializeField] private VRLever windowJoint;
    [SerializeField] private SO_RuntimeReferences runtimeReferences;
    public Transform targetPosition; // The position the intruder will go

    private void OnEnable() {
        windowJoint.OnLeverStateChanged += HandleWindowStateChanged;
        runtimeReferences?.RegisterWindow(this);
    }

    private void OnDisable() {
        windowJoint.OnLeverStateChanged -= HandleWindowStateChanged;
        runtimeReferences?.DeregisterWindow(this);
    }
    private void HandleWindowStateChanged(VRLever.EnumLeverState state) {
        GameManager.Instance.UpdateWindowState(this, state);
        Debug.Log($"Window state changed to {state}");
    }

    public Action<VRLever.EnumLeverState> GetWindowEvent() => windowJoint.OnLeverStateChanged;
    public VRLever.EnumLeverState GetCurrentWindowState() => windowJoint.CurrentState;
}
