using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// A VR-friendly lever component based on a HingeJoint. 
/// Handles interaction, snapping to Open/Closed states, and optimising the physics updates.
/// </summary>
public class VRHingeJoint : MonoBehaviour {

    [Header("Refrences")]
    [SerializeField] private HingeJoint joint;
    [SerializeField] private XRGrabInteractable grabInteractable;

    [Header("Configuration")]
    [Tooltip("Optimize for VR interaction, expected to not need physics updates when lever is in stable state (Open/Closed) and not grabbed.")]
    [SerializeField] private bool optimizeUpdate = true;

    [Tooltip("Determines if a log should be printed")]
    [SerializeField] private bool printDebug = false;

    [Tooltip("Skip automatic positioning of the lever at startup based on startingPercentage. Enable this when the lever's initial rotation is manually set in the scene.")]
    [SerializeField] private bool skipStartRotationInitialization = false;

    [Tooltip("How big of a force the lever will snap shut/open")]
    [SerializeField] private int SpringSnapForce = 100;

    [Tooltip("The starting position of the lever in a percentage value")]
    [SerializeField, Range(0f, 100)] private float startingPercentage = 0;

    [Tooltip("Determines the threshold for the direction the lever will lean towards. Will close if same value or lower")]
    [SerializeField, Range(0, 100)] private float leaningThresholdPercentage = 50;

    [Tooltip("The percentage value where the lever will snap closed")]
    [SerializeField, Range(0f, 100)] private float snapClosedPercentage = 20;

    [Tooltip("The percentage value where the lever will snap open")]
    [SerializeField, Range(0f, 100)] private float snapOpenPercentage = 80;

    [Tooltip("Inverts the state so that the minimum limit is Open and the maximum limit is Closed.")]
    [SerializeField] private bool invertOpenClosedState = false;

    [Header("State")]
    [field: SerializeField] public EnumLeverState CurrentState { get; private set; } = EnumLeverState.None;
    public Action<EnumLeverState> OnLeverStateChanged;
    private EnumLeverState previousState = EnumLeverState.None;

    private JointSpring originalSpring;
    public bool IsGrabbed { get; private set; } = false; // Flag to track if the lever is currently grabbed by the player.
    /// <summary>
    /// Represents the different states the lever can transition through.
    /// </summary>
    public enum EnumLeverState { None, Closed, LeaningClosed, LeaningOpen, Open }

    #region Grab Event Listeners
    /// <summary>
    /// Subscribes to the grab interactable's select and deselect events.
    /// </summary>
    private void OnEnable() {
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }
    /// <summary>
    /// Unsubscribes from the grab interactable's select and deselect events.
    /// </summary>
    private void OnDisable() {
        grabInteractable.selectEntered.RemoveListener(OnGrabbed);
        grabInteractable.selectExited.RemoveListener(OnReleased);
    }
    /// <summary>
    /// Called when the player grabs the lever. 
    /// Reduces snapping force to make moving the lever easier.
    /// </summary>
    /// <param name="args">Event arguments from the XR interactable.</param>
    private void OnGrabbed(SelectEnterEventArgs args) {
        IsGrabbed = true;
        this.SpringSnapForce /= 2; // Decrease by 50% when grabbed to make it easier to move the lever.
    }
    /// <summary>
    /// Called when the player releases the lever. 
    /// Restores snapping force to its original value.
    /// </summary>
    /// <param name="args">Event arguments from the XR interactable.</param>
    private void OnReleased(SelectExitEventArgs args) {
        IsGrabbed = false;
        this.SpringSnapForce *= 2; // Restore to original (increase by 100%)
    }

    #endregion

    /// <summary>
    /// Initializes lever rotation, sets up spring logic, and manages optimization settings.
    /// </summary>
    private IEnumerator Start() {
        this.originalSpring = this.joint.spring;

        if (!this.skipStartRotationInitialization) InitializeStartRotation();
        yield return new WaitForFixedUpdate();
        UpdateSpringBehaviour();
        //StartCoroutine(UpdateSpringBehaviourIEnumerator()); This was less efficient than just calling the method in the FixedUpdate.

        // Disalbe optimization in the begin to ensure the lever's state is correctly updated and events are fired when the game starts,
        // then re-enable it after a short delay to optimize performance during gameplay.
        if (this.optimizeUpdate) {
            this.optimizeUpdate = false;
            yield return new WaitUntil(() => this.CurrentState == EnumLeverState.Open || this.CurrentState == EnumLeverState.Closed);
            this.optimizeUpdate = true;
        }
    }

    /// <summary>
    /// Teleports the hinge to its configured starting rotation percentage using its Rigidbody.
    /// </summary>
    private void InitializeStartRotation() {
        // Get the Rigidbody component attached to the hinge joint to change the angle of the lever.
        Rigidbody rb = this.joint.GetComponent<Rigidbody>();

        // Calculate the initial rotation based on the startingPercentage
        float degreeRotation = this.joint.limits.min + (this.joint.limits.max - this.joint.limits.min) * (this.startingPercentage / 100f);
        Quaternion startRot = Quaternion.AngleAxis(degreeRotation, joint.axis);

        // Set the initial rotation of the lever based on the startingPercentage
        rb.MoveRotation(this.joint.transform.rotation * startRot);
    }

    /// <summary>
    /// Ensures physics-based updates to the lever run consistently. 
    /// Can skip updates for optimization when un-grabbed and at a stable limit.
    /// </summary>
    private void FixedUpdate() {
        if (this.optimizeUpdate) { //If true, skips updates when lever is in stable state(Open/ Closed) and not grabbed.
            if (!IsGrabbed && (this.CurrentState == EnumLeverState.Open || this.CurrentState == EnumLeverState.Closed)) return;
        }
        UpdateSpringBehaviour();
    }


    /// <summary>
    /// Checks the current hinge angle and adjusts the joint spring's target parameters and state appropriately.
    /// </summary>
    private void UpdateSpringBehaviour() {
        float normalizedAngle = (this.joint.angle - this.joint.limits.min) / (this.joint.limits.max - this.joint.limits.min);
        if (this.printDebug) Debug.Log($"Normalized Angle: {normalizedAngle * 100f:F1}%");

        // Adjust the spring settings based on the current angle percentage.
        // Tipping the lever to the angle that its leaning towards and when close to limits making it snap to the limit.
        this.CurrentState = CheckCurrentLeverState(normalizedAngle);
        if (this.CurrentState != this.previousState) {
            this.OnLeverStateChanged?.Invoke(this.CurrentState);
            this.joint.spring = GetSpring(this.CurrentState);
        }
    }

    /// <summary>
    /// Determines the lever's logical state based on its current structural tilt compared to configured thresholds.
    /// </summary>
    /// <param name="normalizedAngle">The calculated rotation angle from 0 to 1.</param>
    /// <returns>The calculated current state enum representation.</returns>
    private EnumLeverState CheckCurrentLeverState(float normalizedAngle) {
        EnumLeverState state;
        if (normalizedAngle <= (this.leaningThresholdPercentage / 100)) {
            state = (normalizedAngle < (this.snapClosedPercentage / 100)) ? EnumLeverState.Closed : EnumLeverState.LeaningClosed;
        } else {
            state = (normalizedAngle > (this.snapOpenPercentage / 100)) ? EnumLeverState.Open : EnumLeverState.LeaningOpen;
        }

        if (this.invertOpenClosedState) {
            if (state == EnumLeverState.Closed) return EnumLeverState.Open;
            if (state == EnumLeverState.LeaningClosed) return EnumLeverState.LeaningOpen;
            if (state == EnumLeverState.Open) return EnumLeverState.Closed;
            if (state == EnumLeverState.LeaningOpen) return EnumLeverState.LeaningClosed;
        }

        return state;
    }

    /// <summary>
    /// Assembles the new physics spring configuration properties based on the lever's new state.
    /// </summary>
    /// <param name="currentState">The new state the lever is transitioning to.</param>
    /// <returns>The new desired target spring.</returns>
    private JointSpring GetSpring(EnumLeverState currentState) {
        this.previousState = this.CurrentState; // Update the previous state before returning the new spring settings.

        float targetClosed = this.invertOpenClosedState ? this.joint.limits.max : this.joint.limits.min;
        float targetOpen = this.invertOpenClosedState ? this.joint.limits.min : this.joint.limits.max;
        //Debug.Log($"{((currentState == EnumLeverState.Closed || currentState == EnumLeverState.LeaningClosed) ? targetClosed : targetOpen)}");

        return new JointSpring {
            spring = (currentState == EnumLeverState.Closed || currentState == EnumLeverState.Open) ? (float)this.SpringSnapForce : this.originalSpring.spring,
            damper = this.originalSpring.damper,
            targetPosition = (currentState == EnumLeverState.Closed || currentState == EnumLeverState.LeaningClosed) ? targetClosed : targetOpen
        };
    }

    /// <summary>
    /// Checks if the lever's optimisation is currently active.
    /// </summary>
    /// <returns>True if optimization is fully enabled</returns>
    public bool GetSmartUpdateEnabled() => this.optimizeUpdate;
    /// <summary>
    /// Suspends physics update optimization for a set time (10 seconds), ensuring it forcefully behaves physically 
    /// whenever this might normally get unexpectedly slept.
    /// </summary>
    public IEnumerator DisableSmartUpdateCorutine() {
        this.optimizeUpdate = false;
        yield return new WaitForSeconds(10f);
        this.optimizeUpdate = true;
    }
}
