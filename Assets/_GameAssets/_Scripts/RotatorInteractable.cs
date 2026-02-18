using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// XR interactable that behaves like a rotatable crank/knob.
/// 
/// When grabbed by an interactor, it calculates the signed angle delta
/// between the previous and current hand direction around a chosen axis,
/// rotates the pivot accordingly, and notifies listeners via OnCrank.
/// </summary>
public class RotatorInteractable : XRBaseInteractable
{
    /// <summary>
    /// Defines which local axis the object rotates around.
    /// </summary>
    public enum RotationAxis { X, Y, Z }

    [SerializeField] private Transform pivot;
    // The transform that will actually be rotated.
    // This is typically the mechanical root of the crank/lever.
    
    [SerializeField] private Transform visual;
    // Optional visual-only transform.
    // Can be used if visuals are separated from the mechanical pivot.
    
    [SerializeField] private RotationAxis rotationAxis = RotationAxis.Y;
    // The axis around which rotation occurs (in local space).
    
    [SerializeField] private bool isRotationClamped;
    // If true, rotation should be limited between minAngle and maxAngle.
    // (Note: clamping logic is not currently implemented in ApplyRotation.)
    
    [SerializeField] private float minAngle = -90f;
    // Minimum allowed rotation angle when clamping is enabled.
    
    [SerializeField] private float maxAngle = 90f;
    // Maximum allowed rotation angle when clamping is enabled.

    private IXRSelectInteractor interactor;
    // The interactor currently grabbing this object.
    
    private Vector3 lastLocalDir;
    // The last projected direction from pivot to interactor,
    // stored in pivot parent local space to calculate delta rotation.

    /// <summary>
    /// Event fired whenever the crank rotates.
    /// Parameter: delta angle (degrees) applied this frame.
    /// </summary>
    public System.Action<float> OnCrank;
    
    /// <summary>
    /// Registers grab event listeners when the object is enabled.
    /// </summary>
    protected override void OnEnable()
    {
        base.OnEnable();
        selectEntered.AddListener(StartGrab);
        selectExited.AddListener(EndGrab);
    }

    /// <summary>
    /// Unregisters grab event listeners when the object is disabled.
    /// Prevents event leaks and invalid callbacks.
    /// </summary>
    protected override void OnDisable()
    {
        selectEntered.RemoveListener(StartGrab);
        selectExited.RemoveListener(EndGrab);
        base.OnDisable();
    }

    /// <summary>
    /// Called when an interactor starts grabbing this object.
    /// Initializes tracking direction for delta angle calculation.
    /// </summary>
    void StartGrab(SelectEnterEventArgs args)
    {
        if (interactor != null) return; 
        interactor = args.interactorObject;
        lastLocalDir = GetProjectedLocalDirection();
    }

    /// <summary>
    /// Called when an interactor releases this object.
    /// Clears active interactor reference.
    /// </summary>
    void EndGrab(SelectExitEventArgs args)
    {
        if (args.interactorObject != interactor) return;
        interactor = null;
    }

    /// <summary>
    /// Processes interaction updates.
    /// During the Dynamic phase, calculates rotation delta while selected.
    /// </summary>
    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);

        if (updatePhase != XRInteractionUpdateOrder.UpdatePhase.Dynamic) return;

        if (!isSelected || interactor == null) return;

        Vector3 currentDir = GetProjectedLocalDirection();

        float delta = SignedAngleBetween(lastLocalDir, currentDir, GetLocalAxis());

        if (Mathf.Abs(delta) > 0.001f)
        {
            ApplyRotation(delta);
            OnCrank?.Invoke(delta);
        }

        lastLocalDir = currentDir;
    }
    
    /// <summary>
    /// Gets the direction from pivot to interactor,
    /// projected onto the plane perpendicular to the rotation axis.
    /// Returned in pivot parent local space.
    /// </summary>
    private Vector3 GetProjectedLocalDirection()
    {
        Vector3 worldDir =
            interactor.GetAttachTransform(this).position - pivot.position;

        Vector3 localDir =
            pivot.parent.InverseTransformDirection(worldDir);

        Vector3 axis = GetLocalAxis();

        // Project onto rotation plane
        localDir -= Vector3.Dot(localDir, axis) * axis;

        return localDir.normalized;
    }

    private float SignedAngleBetween(Vector3 from, Vector3 to, Vector3 axis)
    {
        float angle = Vector3.Angle(from, to);
        float sign = Mathf.Sign(Vector3.Dot(axis, Vector3.Cross(from, to)));
        return angle * sign;
    }
    
    private Vector3 GetLocalAxis()
    {
        return rotationAxis switch
        {
            RotationAxis.X => Vector3.right,
            RotationAxis.Y => Vector3.up,
            RotationAxis.Z => Vector3.forward,
            _ => Vector3.forward
        };
    }
    
    Vector3 GetWorldAxis()
    {
        return rotationAxis switch
        {
            RotationAxis.X => pivot.right,
            RotationAxis.Y => pivot.up,
            RotationAxis.Z => pivot.forward,
            _ => pivot.forward
        };
    }

    void ApplyRotation(float deltaAngle)
    {
        if (!pivot) return;
        
        Vector3 axis = GetWorldAxis();

        pivot.rotation = Quaternion.AngleAxis(deltaAngle, axis) * pivot.rotation;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (!pivot)
            return;

        Gizmos.color = Color.cyan;
        Vector3 axis = rotationAxis switch
        {
            RotationAxis.X => transform.right,
            RotationAxis.Y => transform.up,
            RotationAxis.Z => transform.forward,
            _ => transform.up
        };

        Gizmos.DrawLine(pivot.position, pivot.position + axis * 0.3f);
    }
#endif
    
#if UNITY_EDITOR
    public void EditorRotate(float delta)
    {
        ApplyRotation(delta);
        OnCrank?.Invoke(delta);
    }
#endif
}
