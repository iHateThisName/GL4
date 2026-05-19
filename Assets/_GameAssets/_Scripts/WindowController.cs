using System;
using UnityEngine;

/// <summary>
/// Controls an interactive window in the game, registering it with a global registry 
/// and updating the GameManager when the window's open/close state changes via a VRHingeJoint.
/// </summary>
public class WindowController : MonoBehaviour {
    [field: SerializeField] public VRHingeJoint windowJoint { get; private set; }
    [SerializeField] private SO_WindowRegistryRef windowsRef;
    public Transform TargetPosition;

    /// <summary>
    /// Returns true if the window's lever mechanism is currently being grabbed by the player.
    /// </summary>
    public bool IsGrabbed => windowJoint.IsGrabbed;

    /// <summary>
    /// Subscribes to lever state changes and adds this window to the runtime window registry.
    /// </summary>
    private void OnEnable() {
        windowJoint.OnLeverStateChanged += HandleWindowStateChanged;
        windowsRef?.Add(this);
    }

    /// <summary>
    /// Unsubscribes from lever state changes and removes this window from the runtime window registry.
    /// </summary>
    private void OnDisable() {
        windowJoint.OnLeverStateChanged -= HandleWindowStateChanged;
        windowsRef?.Remove(this);
    }

    /// <summary>
    /// Called when the internal VRHingeJoint transitions to a new state (e.g., Open/Closed).
    /// Updates the GameManager with the new state.
    /// </summary>
    /// <param name="state">The new state of the window lever.</param>
    private void HandleWindowStateChanged(VRHingeJoint.EnumLeverState state) {
        GameManager.Instance.UpdateWindowState(this, state);
        //Debug.Log($"Window state changed to {state}");
    }

    /// <summary>
    /// Gets the event fired when the window lever changes state.
    /// </summary>
    /// <returns>The underlying Action tracking the state changes.</returns>
    public Action<VRHingeJoint.EnumLeverState> GetWindowEvent() => this.windowJoint.OnLeverStateChanged;

    /// <summary>
    /// Gets the current state of the window lever.
    /// </summary>
    /// <returns>The current EnumLeverState.</returns>
    public VRHingeJoint.EnumLeverState GetCurrentWindowState() => this.windowJoint.CurrentState;

    /// <summary>
    /// Checks if the smart update feature is currently enabled on the VRHingeJoint.
    /// </summary>
    /// <returns>True if smart update is enabled, otherwise false.</returns>
    public bool IsVRLeverSmartUpdateEnabled() => this.windowJoint.GetSmartUpdateEnabled();

    /// <summary>
    /// Starts a coroutine on the VRHingeJoint to disable its smart update temporarily or permanently.
    /// </summary>
    public void DisableSmartUpdate() => StartCoroutine(this.windowJoint.DisableSmartUpdateCorutine());
}
