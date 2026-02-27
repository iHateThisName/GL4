using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// Handles crank-style rotation using an XRSimpleInteractable.
/// 
/// When grabbed, the script tracks the interactor's position around a pivot,
/// calculates signed angular delta around a selected axis,
/// applies rotation to the pivot, and notifies listeners via OnCrank.
/// </summary>
public class RotationableInteractable : MonoBehaviour
{
    [System.Serializable]
    public enum RotationAxis
    {
        X,
        Y,
        Z,
    }
    
    // XR interactable used to detect grab/release events.
    [SerializeField] private XRSimpleInteractable interactable;

    // Transform that will be rotated (mechanical pivot point).
    [SerializeField] private Transform pivot;

    // Optional visual-only transform if visuals are separated from logic.
    [SerializeField] private Transform visual;

    // Axis around which the crank rotates.
    [SerializeField] private RotationAxis rotationAxis = RotationAxis.Y;

    // Whether rotation should be limited between minAngle and maxAngle.
    // (Currently not enforced inside ApplyRotation.)
    [SerializeField] private bool isRotationClamped;

    // The interactor currently grabbing this crank.
    private IXRSelectInteractor interactor;

    // Previously recorded projected direction from pivot to interactor.
    // Used to calculate frame-to-frame rotation delta.
    private Vector3 lastLocalDir;

    // True while actively receiving rotation input.
    private bool hasInput;

    /// <summary>
    /// Event fired whenever the crank rotates.
    /// Parameter: delta rotation in degrees for this frame.
    /// </summary>
    public System.Action<float> OnCrank;

    /// <summary>
    /// Subscribes to grab and release events.
    /// </summary>
    private void OnEnable()
    {
        if (this.interactable != null)
        {
            this.interactable.selectEntered.AddListener(StartCranking);
            this.interactable.selectExited.AddListener(EndCranking);
        }
    }

    /// <summary>
    /// Unsubscribes from grab and release events.
    /// Prevents duplicate bindings and event leaks.
    /// </summary>
    private void OnDisable()
    {
        if (this.interactable != null)
        {
            this.interactable.selectEntered.RemoveListener(StartCranking);
            this.interactable.selectExited.RemoveListener(EndCranking);
        }
    }

    /// <summary>
    /// Updates crank rotation each frame while grabbed.
    /// </summary>
    void Update()
    {
        // If nothing is grabbing the crank, do nothing
        if (interactor == null) return;

        // Get interactor's attach transform
        Transform attach = interactor.GetAttachTransform(this.interactable);

        // Process rotation input
        this.UpdateInput(attach.position);
    }

    /// <summary>
    /// Called when the crank is grabbed.
    /// Initializes rotation tracking.
    /// </summary>
    private void StartCranking(SelectEnterEventArgs args)
    {
        // Prevent multiple interactors from controlling the crank
        if (interactor != null) return; 
        interactor = args.interactorObject;

        // Get attach transform position
        Transform attach = interactor.GetAttachTransform(interactable);

        // Initialize last direction for delta calculation
        lastLocalDir = GetProjectedWorldDirection(attach.position);
        hasInput = true;
    }

    /// <summary>
    /// Called when the crank is released.
    /// Stops rotation tracking.
    /// </summary>
    private void EndCranking(SelectExitEventArgs args)
    {
        // Ignore if a different interactor triggered the event
        if (args.interactorObject != interactor) return;

        // reset/freeze crank state
        interactor = null;
        hasInput = false;
    }

    /// <summary>
    /// Processes positional input and applies rotation if needed.
    /// </summary>
    private void UpdateInput(Vector3 position)
    {
        // Ensure we have valid input state
        if (!hasInput) return;

        // Get current projected direction
        Vector3 currentDir = GetProjectedWorldDirection(position);

        // Calculate signed angular difference
        float delta = SignedAngleBetween(lastLocalDir, currentDir, GetWorldAxis());

        // Ignore extremely small rotations
        if (Mathf.Abs(delta) > 0.001f)
        {
            ApplyRotation(delta);
            OnCrank?.Invoke(delta);
        }

        // Store direction for next frame comparison
        lastLocalDir = currentDir;
    }

    /// <summary>
    /// Projects a world position onto the rotation plane
    /// defined by the pivot and selected axis.
    /// </summary>
    Vector3 GetProjectedWorldDirection(Vector3 worldPos)
    {
        // Direction from pivot to interactor
        Vector3 worldDir = worldPos - pivot.position;

        // Rotation axis in world space
        Vector3 axis = GetWorldAxis();

        // Remove component along rotation axis (project onto plane)
        worldDir -= Vector3.Dot(worldDir, axis) * axis;
        return worldDir.normalized;
    }

    /// <summary>
    /// Computes signed angle between two vectors around a given axis.
    /// </summary>
    private float SignedAngleBetween(Vector3 from, Vector3 to, Vector3 axis)
    {
        float angle = Vector3.Angle(from, to);
        float sign = Mathf.Sign(Vector3.Dot(axis, Vector3.Cross(from, to)));
        return angle * sign;
    }

    /// <summary>
    /// Returns selected rotation axis in world space based on pivot.
    /// </summary>
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

    /// <summary>
    /// Applies delta rotation (in degrees) to the pivot.
    /// </summary>
    void ApplyRotation(float deltaAngle)
    {
        // Ensure pivot exists
        if (!pivot) return;

        // Get axis in world space
        Vector3 axis = GetWorldAxis();

        // Apply incremental rotation
        pivot.rotation = Quaternion.AngleAxis(deltaAngle, axis) * pivot.rotation;
    }
}
