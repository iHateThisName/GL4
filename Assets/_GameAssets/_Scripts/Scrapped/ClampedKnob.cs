using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// A clamped, snapping XR knob that rotates around its local forward (Z) axis.
/// Outputs a normalized value (0-1) and current step index.
/// Uses three input sources (position offset, controller forward/up) with accumulated
/// rotation tracking to avoid frame-delta instability.
/// </summary>
public class ClampedKnob : XRBaseInteractable
{
    const float k_ModeSwitchDeadZone = 0.1f;

    /// <summary>
    /// Tracks rotation from a grab-time base, accumulating offsets to handle full
    /// rotation ranges while minimising floating-point error. Angles are computed in
    /// the XY plane using Atan2(y, x).
    /// </summary>
    struct TrackedRotation
    {
        float m_BaseAngle;
        float m_CurrentOffset;
        float m_AccumulatedAngle;

        public float totalOffset => m_AccumulatedAngle + m_CurrentOffset;

        public void Reset()
        {
            m_BaseAngle = 0f;
            m_CurrentOffset = 0f;
            m_AccumulatedAngle = 0f;
        }

        /// <summary>Bakes in any accumulated offset and sets a new base from the given XY direction.</summary>
        public void SetBaseFromVector(Vector3 direction)
        {
            m_AccumulatedAngle += m_CurrentOffset;
            m_BaseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            m_CurrentOffset = 0f;
        }

        /// <summary>Updates the current offset toward the given XY direction. Re-anchors when offset exceeds 90°.</summary>
        public void SetTargetFromVector(Vector3 direction)
        {
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            m_CurrentOffset = ShortestAngleDistance(m_BaseAngle, targetAngle, 360f);

            // Re-anchor so accumulated rotation can exceed 180°
            if (Mathf.Abs(m_CurrentOffset) > 90f)
            {
                m_BaseAngle = targetAngle;
                m_AccumulatedAngle += m_CurrentOffset;
                m_CurrentOffset = 0f;
            }
        }

        static float ShortestAngleDistance(float start, float end, float max)
        {
            float delta = end - start;
            float sign = Mathf.Sign(delta);
            delta = Mathf.Abs(delta) % max;
            if (delta > max * 0.5f)
                delta = -(max - delta);
            return delta * sign;
        }
    }

    /* =======================
     * Serialized Fields
     * ======================= */

    [Header("Knob Settings")]
    [Tooltip("The transform that visually rotates. If null, uses this transform.")]
    [SerializeField] private Transform handle;

    [Tooltip("Rotation angle at value 0 (left limit)")]
    [SerializeField] private float minAngle = -140f;

    [Tooltip("Rotation angle at value 1 (right limit)")]
    [SerializeField] private float maxAngle = 140f;

    [Tooltip("Invert the rotation direction")]
    [SerializeField] private bool invertRotation = false;

    [Tooltip("Interactor must be at least this far from the handle center (world units) to use position tracking")]
    [SerializeField] private float positionTrackedRadius = 0.1f;

    [Tooltip("Multiplier for controller rotation (twist/forward/up vector) input")]
    [SerializeField] private float twistSensitivity = 1.5f;

    [Header("Steps")]
    [Tooltip("Number of discrete positions/channels. Must be >= 2.")]
    [SerializeField] private int steps = 10;

    [Header("Editor Testing")]
    [Tooltip("Amount to rotate when using context menu (degrees)")]
    [SerializeField] private float debugRotationStep = 30f;

    [Tooltip("How far forward to draw the gizmo arc")]
    [SerializeField] private float gizmoForwardOffset = 0.1f;

    [Header("Events")]
    [SerializeField] private UnityEvent<int> onStepChanged = new UnityEvent<int>();

    /* =======================
     * Private Fields
     * ======================= */

    private IXRSelectInteractor interactor;
    private float currentAngle;
    private int currentStep = -1;

    private float baseKnobAngle;
    private bool positionDriven;
    private bool upVectorDriven;
    private TrackedRotation positionAngles;
    private TrackedRotation upVectorAngles;
    private TrackedRotation forwardVectorAngles;

    /* =======================
     * Properties
     * ======================= */

    /// <summary>Current normalized value (0 to 1)</summary>
    public float Value => AngleToValue(currentAngle);

    /// <summary>Current rotation angle in degrees</summary>
    public float Angle => currentAngle;

    /// <summary>Current step index (0 to Steps-1)</summary>
    public int CurrentStep => currentStep;

    /// <summary>Total number of steps</summary>
    public int Steps
    {
        get => steps;
        set => steps = Mathf.Max(2, value);
    }

    /// <summary>The step index that corresponds to angle 0</summary>
    public int StepAtAngleZero => AngleToStep(0f);

    /// <summary>Angle increment per step</summary>
    public float AnglePerStep => (maxAngle - minAngle) / (steps - 1);

    /// <summary>Event fired when step changes</summary>
    public UnityEvent<int> OnStepChanged => onStepChanged;

    /// <summary>The visual handle transform</summary>
    public Transform Handle
    {
        get => handle;
        set => handle = value;
    }

    /* =======================
     * Unity Lifecycle
     * ======================= */

    protected override void Awake()
    {
        base.Awake();

        if (handle == null)
            handle = transform;

        // Initialize to angle 0 in Awake so Radio.Start() can override it
        currentAngle = 0f;
        SnapToNearestStep();
        UpdateVisual();
        currentStep = AngleToStep(currentAngle);
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        selectEntered.AddListener(OnGrab);
        selectExited.AddListener(OnRelease);
    }

    protected override void OnDisable()
    {
        selectEntered.RemoveListener(OnGrab);
        selectExited.RemoveListener(OnRelease);
        base.OnDisable();
    }

    /* =======================
     * XR Interactable Overrides
     * ======================= */

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);

        if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic && isSelected)
            UpdateRotation();
    }

    public override Transform GetAttachTransform(IXRInteractor interactor)
    {
        return handle != null ? handle : base.GetAttachTransform(interactor);
    }

    /* =======================
     * Grab Handling
     * ======================= */

    private void OnGrab(SelectEnterEventArgs args)
    {
        interactor = args.interactorObject;

        positionAngles.Reset();
        upVectorAngles.Reset();
        forwardVectorAngles.Reset();

        baseKnobAngle = currentAngle;
        UpdateRotation(freshCheck: true);
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        interactor = null;
    }

    /* =======================
     * Rotation Logic
     * ======================= */

    private void UpdateRotation(bool freshCheck = false)
    {
        if (interactor == null) return;

        var interactorTransform = interactor.GetAttachTransform(this);

        // --- Source 1: position offset ---
        // Vector from handle center to interactor, projected onto local XY (zero out Z).
        var localOffset = transform.InverseTransformVector(interactorTransform.position - handle.position);
        localOffset.z = 0f;
        // World-space magnitude of the projected offset, used for radius threshold check.
        var radiusOffset = transform.TransformVector(localOffset).magnitude;
        localOffset.Normalize();

        // --- Sources 2 & 3: controller forward / up vectors ---
        // Both projected onto local XY (zero out Z).
        var localForward = transform.InverseTransformDirection(interactorTransform.forward);
        // How much the controller points along the knob's Z axis — drives forward vs up switch.
        var localZ = Mathf.Abs(localForward.z);
        localForward.z = 0f;
        localForward.Normalize();

        var localUp = transform.InverseTransformDirection(interactorTransform.up);
        localUp.z = 0f;
        localUp.Normalize();

        // --- Mode switching: position ---
        // Apply hysteresis so we don't flicker at the boundary.
        if (positionDriven && !freshCheck)
            radiusOffset *= 1f + k_ModeSwitchDeadZone;

        if (radiusOffset >= positionTrackedRadius)
        {
            if (!positionDriven || freshCheck)
            {
                positionAngles.SetBaseFromVector(localOffset);
                positionDriven = true;
            }
        }
        else
            positionDriven = false;

        // --- Mode switching: forward vs up vector ---
        // When the controller points mostly along knob Z, forward projects to near-zero
        // in XY — switch to up vector tracking instead.
        if (!freshCheck)
        {
            if (!upVectorDriven)
                localZ *= 1f - k_ModeSwitchDeadZone * 0.5f;
            else
                localZ *= 1f + k_ModeSwitchDeadZone * 0.5f;
        }

        if (localZ > 0.707f)
        {
            if (!upVectorDriven || freshCheck)
            {
                upVectorAngles.SetBaseFromVector(localUp);
                upVectorDriven = true;
            }
        }
        else
        {
            if (upVectorDriven || freshCheck)
            {
                forwardVectorAngles.SetBaseFromVector(localForward);
                upVectorDriven = false;
            }
        }

        // --- Update active sources ---
        if (positionDriven)
            positionAngles.SetTargetFromVector(localOffset);

        if (upVectorDriven)
            upVectorAngles.SetTargetFromVector(localUp);
        else
            forwardVectorAngles.SetTargetFromVector(localForward);

        // --- Compute new angle ---
        float totalRotation = (upVectorAngles.totalOffset + forwardVectorAngles.totalOffset) * twistSensitivity
                              + positionAngles.totalOffset;

        if (invertRotation)
            totalRotation = -totalRotation;

        float newAngle = Mathf.Clamp(baseKnobAngle - totalRotation, minAngle, maxAngle);

        SnapAngle(ref newAngle);
        newAngle = Mathf.Clamp(newAngle, minAngle, maxAngle);

        if (Mathf.Approximately(newAngle, currentAngle)) return;

        currentAngle = newAngle;
        UpdateVisual();

        int newStep = AngleToStep(currentAngle);
        if (newStep != currentStep)
        {
            currentStep = newStep;
            onStepChanged.Invoke(currentStep);
        }
    }

    private void SnapAngle(ref float angle)
    {
        if (steps <= 1) return;

        float range = maxAngle - minAngle;
        float stepAngle = range / (steps - 1);
        float normalized = (angle - minAngle) / range;
        int stepIndex = Mathf.RoundToInt(normalized * (steps - 1));
        angle = minAngle + stepIndex * stepAngle;
    }

    private void SnapToNearestStep()
    {
        SnapAngle(ref currentAngle);
        currentAngle = Mathf.Clamp(currentAngle, minAngle, maxAngle);
    }

    private int AngleToStep(float angle)
    {
        if (steps <= 1) return 0;

        float range = maxAngle - minAngle;
        float normalized = (angle - minAngle) / range;
        int stepIndex = Mathf.RoundToInt(normalized * (steps - 1));
        return Mathf.Clamp(stepIndex, 0, steps - 1);
    }

    private float StepToAngle(int step)
    {
        if (steps <= 1) return minAngle;

        step = Mathf.Clamp(step, 0, steps - 1);
        float range = maxAngle - minAngle;
        float stepAngle = range / (steps - 1);
        return minAngle + step * stepAngle;
    }

    /* =======================
     * Visual Update
     * ======================= */

    private void UpdateVisual()
    {
        if (handle == null) return;

        // Rotate around local Z axis (forward)
        handle.localRotation = Quaternion.Euler(0f, 0f, -currentAngle);
    }

    /* =======================
     * Value Conversion
     * ======================= */

    private float AngleToValue(float angle)
    {
        return Mathf.InverseLerp(minAngle, maxAngle, angle);
    }

    private float ValueToAngle(float value)
    {
        return Mathf.Lerp(minAngle, maxAngle, value);
    }

    /* =======================
     * Public API
     * ======================= */

    /// <summary>Sets the knob to a specific step index.</summary>
    public void SetStep(int step)
    {
        step = Mathf.Clamp(step, 0, steps - 1);
        currentAngle = StepToAngle(step);
        UpdateVisual();

        if (step != currentStep)
        {
            currentStep = step;
            onStepChanged.Invoke(currentStep);
        }
    }

    /// <summary>Sets the knob to a specific normalized value (0-1).</summary>
    public void SetValue(float value)
    {
        value = Mathf.Clamp01(value);
        currentAngle = ValueToAngle(value);
        SnapToNearestStep();
        UpdateVisual();

        int newStep = AngleToStep(currentAngle);
        if (newStep != currentStep)
        {
            currentStep = newStep;
            onStepChanged.Invoke(currentStep);
        }
    }

    /// <summary>Sets the knob to a specific angle (clamped and snapped).</summary>
    public void SetAngle(float angle)
    {
        currentAngle = Mathf.Clamp(angle, minAngle, maxAngle);
        SnapToNearestStep();
        UpdateVisual();

        int newStep = AngleToStep(currentAngle);
        if (newStep != currentStep)
        {
            currentStep = newStep;
            onStepChanged.Invoke(currentStep);
        }
    }

    /* =======================
     * Editor
     * ======================= */

    private void OnValidate()
    {
        if (minAngle > maxAngle)
            minAngle = maxAngle;

        if (steps < 2)
            steps = 2;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 forward = Vector3.forward;
        Vector3 up = Vector3.up;
        Vector3 center = transform.position + transform.forward * gizmoForwardOffset;

        float radius = 0.05f;

        Gizmos.color = Color.red;
        Gizmos.DrawLine(center, center + Quaternion.AngleAxis(minAngle, forward) * up * radius);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(center, center + Quaternion.AngleAxis(maxAngle, forward) * up * radius);

        Gizmos.color = Color.white;
        Gizmos.DrawLine(center, center + up * radius * 0.8f);

        if (steps >= 2)
        {
            for (int i = 0; i < steps; i++)
            {
                float stepAngle = StepToAngle(i);
                Vector3 stepDir = Quaternion.AngleAxis(stepAngle, forward) * up;

                if (i == currentStep)
                {
                    Gizmos.color = new Color(1f, 0.6f, 0f);
                    Gizmos.DrawSphere(center + stepDir * radius, 0.004f);
                }
                else
                {
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawSphere(center + stepDir * radius, 0.002f);
                }
            }
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(center, center + Quaternion.AngleAxis(currentAngle, forward) * up * radius * 1.2f);

        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        DrawArc(center, forward, up, minAngle, maxAngle, radius);
    }

    private void DrawArc(Vector3 center, Vector3 axis, Vector3 startDir, float fromAngle, float toAngle, float radius)
    {
        int segments = 32;
        float angleRange = toAngle - fromAngle;

        for (int i = 0; i < segments; i++)
        {
            float a1 = fromAngle + (i / (float)segments) * angleRange;
            float a2 = fromAngle + ((i + 1) / (float)segments) * angleRange;

            Vector3 p1 = center + Quaternion.AngleAxis(a1, axis) * startDir * radius;
            Vector3 p2 = center + Quaternion.AngleAxis(a2, axis) * startDir * radius;

            Gizmos.DrawLine(p1, p2);
        }
    }

    /* =======================
     * Context Menu (Editor Testing)
     * ======================= */

    [ContextMenu("Rotate Left")]
    private void DebugRotateLeft() => DebugRotate(-GetDebugStep());

    [ContextMenu("Rotate Right")]
    private void DebugRotateRight() => DebugRotate(GetDebugStep());

    [ContextMenu("Set to Angle Zero")]
    private void DebugSetToAngleZero() => DebugSetAngle(0f);

    [ContextMenu("Next Step")]
    private void DebugNextStep()
    {
        SetStep(Mathf.Min(currentStep + 1, steps - 1));
        Debug.Log($"[ClampedKnob] Step: {currentStep} | Angle: {currentAngle:F1}°");
    }

    [ContextMenu("Previous Step")]
    private void DebugPrevStep()
    {
        SetStep(Mathf.Max(currentStep - 1, 0));
        Debug.Log($"[ClampedKnob] Step: {currentStep} | Angle: {currentAngle:F1}°");
    }

    [ContextMenu("Set to Step at Angle Zero")]
    private void DebugSetToStepAtAngleZero()
    {
        SetStep(StepAtAngleZero);
        Debug.Log($"[ClampedKnob] Step at angle 0: {StepAtAngleZero} | Angle: {currentAngle:F1}°");
    }

    private float GetDebugStep() => debugRotationStep > 0f ? debugRotationStep : 30f;

    private void DebugRotate(float delta)
    {
        float newAngle = Mathf.Clamp(currentAngle + delta, minAngle, maxAngle);
        SnapAngle(ref newAngle);
        newAngle = Mathf.Clamp(newAngle, minAngle, maxAngle);

        if (Mathf.Approximately(newAngle, currentAngle)) return;

        currentAngle = newAngle;
        UpdateVisual();

        int newStep = AngleToStep(currentAngle);
        if (newStep != currentStep)
        {
            currentStep = newStep;
            if (Application.isPlaying)
                onStepChanged.Invoke(currentStep);
        }

        Debug.Log($"[ClampedKnob] Step: {currentStep} | Angle: {currentAngle:F1}°");
    }

    private void DebugSetAngle(float angle)
    {
        currentAngle = Mathf.Clamp(angle, minAngle, maxAngle);
        SnapToNearestStep();
        UpdateVisual();

        int newStep = AngleToStep(currentAngle);
        if (newStep != currentStep)
        {
            currentStep = newStep;
            if (Application.isPlaying)
                onStepChanged.Invoke(currentStep);
        }

        Debug.Log($"[ClampedKnob] Step: {currentStep} | Angle: {currentAngle:F1}°");
    }
}
