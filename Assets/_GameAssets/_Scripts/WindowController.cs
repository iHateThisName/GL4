using System;
using UnityEngine;

public class WindowController : MonoBehaviour
{
    [SerializeField] private VRLever windowJoint;
    [SerializeField] private SO_WindowRegistryRef windowsRef;
    public Transform TargetPosition;

    private void OnEnable()
    {
        windowJoint.OnLeverStateChanged += HandleWindowStateChanged;
        windowsRef?.Add(this);
    }

    private void OnDisable()
    {
        windowJoint.OnLeverStateChanged -= HandleWindowStateChanged;
        windowsRef?.Remove(this);
    }

    private void HandleWindowStateChanged(VRLever.EnumLeverState state)
    {
        GameManager.Instance.UpdateWindowState(this, state);
        Debug.Log($"Window state changed to {state}");
    }

    public Action<VRLever.EnumLeverState> GetWindowEvent() => windowJoint.OnLeverStateChanged;
    public VRLever.EnumLeverState GetCurrentWindowState() => windowJoint.CurrentState;
    public bool IsVRLeverSmartUpdateEnabled() => windowJoint.GetSmartUpdateEnabled();
}
