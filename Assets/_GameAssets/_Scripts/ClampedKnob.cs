using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// A clamped, snapping XR knob that rotates around its local forward (Z) axis.
/// Outputs a normalized value (0-1) based on rotation.
/// Uses delta-based tracking to avoid accumulation/buffer problems.
/// </summary>
public class ClampedKnob : XRBaseInteractable
{
    /* =======================
     * Serialized Fields
     * ======================= */

    [Header("Knob Settings")]
    [SerializeField]
    [Tooltip("The transform that visually rotates. If null, uses this transform.")]
    private Transform handle;

    [SerializeField]
    [Tooltip("Rotation angle at value 0 (left limit)")]
    private float minAngle = -140f;

    [SerializeField]
    [Tooltip("Rotation angle at value 1 (right limit)")]
    private float maxAngle = 140f;

    [SerializeField]
    [Tooltip("Starting normalized value (0-1)")]
    [Range(0f, 1f)]
    private float startingValue = 0.5f;

    [Header("Snapping")]
    [SerializeField]
    [Tooltip("Number of snap positions. Set to 0 for smooth rotation.")]
    private int snapPositions = 0;

    [Header("Events")]
    [SerializeField]
    private UnityEvent<float> onValueChanged = new UnityEvent<float>();

    /* =======================
     * Private Fields
     * ======================= */

    private IXRSelectInteractor interactor;
    private Vector3 lastProjectedDir;
    private float currentAngle;

    /* =======================
     * Properties
     * ======================= */

    /// <summary>Current normalized value (0 to 1)</summary>
    public float Value => AngleToValue(currentAngle);

    /// <summary>Current rotation angle in degrees</summary>
    public float Angle => currentAngle;

    /// <summary>Event fired when value changes</summary>
    public UnityEvent<float> OnValueChanged => onValueChanged;

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
    }

    private void Start()
    {
        // Initialize from starting value
        currentAngle = ValueToAngle(startingValue);
        ApplySnapping(ref currentAngle);
        UpdateVisual();
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

        // Calculate new angle with clamping
        float newAngle = Mathf.Clamp(currentAngle + delta, minAngle, maxAngle);

        // Apply snapping
        ApplySnapping(ref newAngle);

        // Ensure still within bounds after snapping
        newAngle = Mathf.Clamp(newAngle, minAngle, maxAngle);

        if (Mathf.Approximately(newAngle, currentAngle)) return;

        currentAngle = newAngle;
        UpdateVisual();
        onValueChanged.Invoke(Value);
    }

    private void ApplySnapping(ref float angle)
    {
        if (snapPositions <= 1) return;

        float range = maxAngle - minAngle;
        float snapAngle = range / (snapPositions - 1);
        float normalized = (angle - minAngle) / range;
        int snapIndex = Mathf.RoundToInt(normalized * (snapPositions - 1));
        angle = minAngle + snapIndex * snapAngle;
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
    /// Sets the knob to a specific normalized value (0-1).
    /// </summary>
    public void SetValue(float value)
    {
        value = Mathf.Clamp01(value);
        currentAngle = ValueToAngle(value);
        ApplySnapping(ref currentAngle);
        UpdateVisual();
        onValueChanged.Invoke(Value);
    }

    /// <summary>
    /// Sets the knob to a specific angle (clamped and snapped).
    /// </summary>
    public void SetAngle(float angle)
    {
        currentAngle = Mathf.Clamp(angle, minAngle, maxAngle);
        ApplySnapping(ref currentAngle);
        UpdateVisual();
        onValueChanged.Invoke(Value);
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

        if (snapPositions < 0)
        {
            snapPositions = 0;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center = transform.position;
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        float radius = 0.05f;

        // Draw min angle
        Gizmos.color = Color.red;
        Vector3 minDir = Quaternion.AngleAxis(-minAngle, forward) * right;
        Gizmos.DrawLine(center, center + minDir * radius);

        // Draw max angle
        Gizmos.color = Color.green;
        Vector3 maxDir = Quaternion.AngleAxis(-maxAngle, forward) * right;
        Gizmos.DrawLine(center, center + maxDir * radius);

        // Draw current angle
        Gizmos.color = Color.yellow;
        Vector3 currentDir = Quaternion.AngleAxis(-currentAngle, forward) * right;
        Gizmos.DrawLine(center, center + currentDir * radius * 1.2f);

        // Draw arc
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        DrawArc(center, forward, right, minAngle, maxAngle, radius);
    }

    private void DrawArc(Vector3 center, Vector3 axis, Vector3 startDir, float fromAngle, float toAngle, float radius)
    {
        int segments = 32;
        float angleRange = toAngle - fromAngle;

        for (int i = 0; i < segments; i++)
        {
            float a1 = fromAngle + (i / (float)segments) * angleRange;
            float a2 = fromAngle + ((i + 1) / (float)segments) * angleRange;

            Vector3 p1 = center + Quaternion.AngleAxis(-a1, axis) * startDir * radius;
            Vector3 p2 = center + Quaternion.AngleAxis(-a2, axis) * startDir * radius;

            Gizmos.DrawLine(p1, p2);
        }
    }
}
