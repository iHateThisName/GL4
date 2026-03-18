using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class VRLever : MonoBehaviour {

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
    [SerializeField] private EnumLeverState currentState = EnumLeverState.None;
    private EnumLeverState previousState = EnumLeverState.None;

    private JointSpring originalSpring;
    private bool isGrabbed = false; // Flag to track if the lever is currently grabbed by the player.
    private enum EnumLeverState { None, Closed, LeaningClosed, LeaningOpen, Open }

    #region Grab Event Listeners
    private void OnEnable() {
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }
    private void OnDisable() {
        grabInteractable.selectEntered.RemoveListener(OnGrabbed);
        grabInteractable.selectExited.RemoveListener(OnReleased);
    }
    private void OnGrabbed(SelectEnterEventArgs args) {
        isGrabbed = true;
        this.SpringSnapForce /= 2; // Decrease by 50% when grabbed to make it easier to move the lever.
    }
    private void OnReleased(SelectExitEventArgs args) {
        isGrabbed = false;
        this.SpringSnapForce *= 2; // Restore to original (increase by 100%)
    }

    #endregion

    private IEnumerator Start() {
        this.originalSpring = this.joint.spring;

        if (!this.skipStartRotationInitialization) InitializeStartRotation();
        yield return new WaitForFixedUpdate();
        UpdateSpringBehaviour();
        //StartCoroutine(UpdateSpringBehaviourIEnumerator()); This was less efficient than just calling the method in the FixedUpdate.
    }

    private void InitializeStartRotation() {
        // Get the Rigidbody component attached to the hinge joint to change the angle of the lever.
        Rigidbody rb = this.joint.GetComponent<Rigidbody>();

        // Calculate the initial rotation based on the startingPercentage
        float degreeRotation = this.joint.limits.min + (this.joint.limits.max - this.joint.limits.min) * (this.startingPercentage / 100f);
        Quaternion startRot = Quaternion.AngleAxis(degreeRotation, joint.axis);

        // Set the initial rotation of the lever based on the startingPercentage
        rb.MoveRotation(this.joint.transform.rotation * startRot);
    }

    private void FixedUpdate() {
        if (this.optimizeUpdate) { //If true, skips updates when lever is in stable state(Open/ Closed) and not grabbed.
            if (!isGrabbed & (this.currentState == EnumLeverState.Open || this.currentState == EnumLeverState.Closed)) return;
        }
        UpdateSpringBehaviour();
    }


    private void UpdateSpringBehaviour() {
        float normalizedAngle = (this.joint.angle - this.joint.limits.min) / (this.joint.limits.max - this.joint.limits.min);
        if (this.printDebug) Debug.Log($"Normalized Angle: {normalizedAngle * 100f:F1}%");

        // Adjust the spring settings based on the current angle percentage.
        // Tipping the lever to the angle that its leaning towards and when close to limits making it snap to the limit.
        this.currentState = CheckCurrentLeverState(normalizedAngle);
        if (this.currentState != this.previousState) this.joint.spring = GetSpring(this.currentState);
    }

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

    private JointSpring GetSpring(EnumLeverState currentState) {
        this.previousState = this.currentState; // Update the previous state before returning the new spring settings.

        float targetClosed = this.invertOpenClosedState ? this.joint.limits.max : this.joint.limits.min;
        float targetOpen = this.invertOpenClosedState ? this.joint.limits.min : this.joint.limits.max;

        return new JointSpring {
            spring = (currentState == EnumLeverState.Closed || currentState == EnumLeverState.Open) ? (float)this.SpringSnapForce : this.originalSpring.spring,
            damper = this.originalSpring.damper,
            targetPosition = (currentState == EnumLeverState.Closed || currentState == EnumLeverState.LeaningClosed) ? targetClosed : targetOpen
        };
    }

    #region Unused Code
    private IEnumerator UpdateSpringBehaviourIEnumerator() {
        while (true) {
            UpdateSpringBehaviour();
            yield return new WaitForSeconds(2f);
        }
    }
    private void ApplySpringBehaviour(float normalizedAngle) {
        if (normalizedAngle < 0.50f) {
            this.joint.spring = new JointSpring {
                spring = normalizedAngle < 0.20f ? 100f : this.originalSpring.spring,
                damper = this.originalSpring.damper,
                targetPosition = this.joint.limits.min
            };

        } else {
            this.joint.spring = new JointSpring {
                spring = normalizedAngle > 0.80f ? 100f : this.originalSpring.spring,
                damper = this.originalSpring.damper,
                targetPosition = this.joint.limits.max
            };
        }

        this.previousState = this.currentState;
    }
    #endregion 
}
