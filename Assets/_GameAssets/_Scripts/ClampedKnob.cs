using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// A clamped, snapping XR knob that rotates around its local forward (Z) axis.
/// Outputs a normalized value (0-1) and current step index.
/// Uses delta-based tracking to avoid accumulation/buffer problems.
/// </summary>
public class ClampedKnob : XRBaseInteractable
{
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

    [Tooltip("Maximum rotation per frame (prevents wrap-around jumps)")]
    [SerializeField] private float maxDeltaPerFrame = 45f;

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
    private Vector3 lastProjectedDir;
    private float currentAngle;
    private int currentStep = -1;

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
        {
            handle = transform;
        }

        // Initialize to angle 0 in Awake, so Radio.Start() can override it
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
        {
            UpdateRotation();
        }
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
        Transform attach = interactor.GetAttachTransform(this);
        lastProjectedDir = GetProjectedDirection(attach.position);
    }

    private void OnRelease(SelectExitEventArgs args)
    {
        interactor = null;
    }

    /* =======================
     * Rotation Logic
     * ======================= */

    private void UpdateRotation()
    {
        if (interactor == null) return;

        Transform attach = interactor.GetAttachTransform(this);
        Vector3 currentDir = GetProjectedDirection(attach.position);

        float delta = SignedAngle(lastProjectedDir, currentDir);

        // CRITICAL: Always update lastProjectedDir regardless of clamping.
        // This prevents the accumulation/buffer problem.
        lastProjectedDir = currentDir;

        if (Mathf.Abs(delta) < 0.001f) return;

        // Clamp delta to prevent wrap-around jumps (e.g., -180 to 180)
        delta = Mathf.Clamp(delta, -maxDeltaPerFrame, maxDeltaPerFrame);

        // Invert rotation direction if needed
        if (invertRotation)
        {
            delta = -delta;
        }

        // Calculate new angle with clamping
        float newAngle = Mathf.Clamp(currentAngle + delta, minAngle, maxAngle);

        // Apply snapping
        SnapAngle(ref newAngle);

        // Ensure still within bounds after snapping
        newAngle = Mathf.Clamp(newAngle, minAngle, maxAngle);

        if (Mathf.Approximately(newAngle, currentAngle)) return;

        currentAngle = newAngle;
        UpdateVisual();

        // Check if step changed
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
     * Geometry Helpers
     * ======================= */

    private Vector3 GetProjectedDirection(Vector3 worldPos)
    {
        // Get direction from knob center to interactor in local space
        Vector3 localPos = transform.InverseTransformPoint(worldPos);

        // Project onto XY plane (perpendicular to Z/forward axis)
        localPos.z = 0f;

        return localPos.normalized;
    }

    private float SignedAngle(Vector3 from, Vector3 to)
    {
        // 2D signed angle in the XY plane
        float angle = Vector2.SignedAngle(
            new Vector2(from.x, from.y),
            new Vector2(to.x, to.y)
        );
        return angle;
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

    /// <summary>
    /// Sets the knob to a specific step index.
    /// </summary>
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

    /// <summary>
    /// Sets the knob to a specific normalized value (0-1).
    /// </summary>
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

    /// <summary>
    /// Sets the knob to a specific angle (clamped and snapped).
    /// </summary>
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
        {
            minAngle = maxAngle;
        }

        if (steps < 2)
        {
            steps = 2;
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Use world axes so the gizmo stays static
        // Up is the 0° direction (12 o'clock), forward is the rotation axis
        Vector3 forward = Vector3.forward;
        Vector3 up = Vector3.up;
        Vector3 center = transform.position + transform.forward * gizmoForwardOffset;

        float radius = 0.05f;

        // Draw min angle (left limit) - red line
        Gizmos.color = Color.red;
        Vector3 minDir = Quaternion.AngleAxis(minAngle, forward) * up;
        Gizmos.DrawLine(center, center + minDir * radius);

        // Draw max angle (right limit) - green line
        Gizmos.color = Color.green;
        Vector3 maxDir = Quaternion.AngleAxis(maxAngle, forward) * up;
        Gizmos.DrawLine(center, center + maxDir * radius);

        // Draw zero/center angle - white line (12 o'clock)
        Gizmos.color = Color.white;
        Gizmos.DrawLine(center, center + up * radius * 0.8f);

        // Draw step positions - dots with current step highlighted
        if (steps >= 2)
        {
            for (int i = 0; i < steps; i++)
            {
                float stepAngle = StepToAngle(i);
                Vector3 stepDir = Quaternion.AngleAxis(stepAngle, forward) * up;

                if (i == currentStep)
                {
                    // Current step - larger yellow/orange sphere
                    Gizmos.color = new Color(1f, 0.6f, 0f); // Orange
                    Gizmos.DrawSphere(center + stepDir * radius, 0.004f);
                }
                else
                {
                    // Other steps - small cyan dots
                    Gizmos.color = Color.cyan;
                    Gizmos.DrawSphere(center + stepDir * radius, 0.002f);
                }
            }
        }

        // Draw current angle - yellow line (longer)
        Gizmos.color = Color.yellow;
        Vector3 currentDir = Quaternion.AngleAxis(currentAngle, forward) * up;
        Gizmos.DrawLine(center, center + currentDir * radius * 1.2f);

        // Draw arc showing valid rotation range
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
    private void DebugRotateLeft()
    {
        DebugRotate(-GetDebugStep());
    }

    [ContextMenu("Rotate Right")]
    private void DebugRotateRight()
    {
        DebugRotate(GetDebugStep());
    }

    [ContextMenu("Set to Angle Zero")]
    private void DebugSetToAngleZero()
    {
        DebugSetAngle(0f);
    }

    [ContextMenu("Next Step")]
    private void DebugNextStep()
    {
        int nextStep = Mathf.Min(currentStep + 1, steps - 1);
        SetStep(nextStep);
        Debug.Log($"[ClampedKnob] Step: {currentStep} | Angle: {currentAngle:F1}°");
    }

    [ContextMenu("Previous Step")]
    private void DebugPrevStep()
    {
        int prevStep = Mathf.Max(currentStep - 1, 0);
        SetStep(prevStep);
        Debug.Log($"[ClampedKnob] Step: {currentStep} | Angle: {currentAngle:F1}°");
    }

    [ContextMenu("Set to Step at Angle Zero")]
    private void DebugSetToStepAtAngleZero()
    {
        SetStep(StepAtAngleZero);
        Debug.Log($"[ClampedKnob] Step at angle 0: {StepAtAngleZero} | Angle: {currentAngle:F1}°");
    }

    private float GetDebugStep()
    {
        return debugRotationStep > 0f ? debugRotationStep : 30f;
    }

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
            {
                onStepChanged.Invoke(currentStep);
            }
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
            {
                onStepChanged.Invoke(currentStep);
            }
        }

        Debug.Log($"[ClampedKnob] Step: {currentStep} | Angle: {currentAngle:F1}°");
    }
}
