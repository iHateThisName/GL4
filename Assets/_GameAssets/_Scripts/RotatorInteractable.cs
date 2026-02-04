using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class RotatorInteractable : XRBaseInteractable
{
    public enum RotationAxis { X, Y, Z }

    [SerializeField] private Transform pivot;        // rotates
    [SerializeField] private Transform visual;       // optional, purely visual
    [SerializeField] private RotationAxis rotationAxis = RotationAxis.Y;
    [SerializeField] private bool isRotationClamped;
    [SerializeField] private float minAngle = -90f;
    [SerializeField] private float maxAngle = 90f;

    private IXRSelectInteractor interactor;
    private Vector3 lastLocalDir;
    private float accumulatedAngle;

    public System.Action<float> OnCrank;

    protected override void OnEnable()
    {
        base.OnEnable();
        selectEntered.AddListener(StartGrab);
        selectExited.AddListener(EndGrab);
    }

    protected override void OnDisable()
    {
        selectEntered.RemoveListener(StartGrab);
        selectExited.RemoveListener(EndGrab);
        base.OnDisable();
    }

    void StartGrab(SelectEnterEventArgs args)
    {
        if (interactor != null) return; 
        interactor = args.interactorObject;
        lastLocalDir = GetProjectedLocalDirection();
    }

    void EndGrab(SelectExitEventArgs args)
    {
        if (args.interactorObject != interactor) return;
        interactor = null;
    }

    public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
    {
        base.ProcessInteractable(updatePhase);

        if (updatePhase != XRInteractionUpdateOrder.UpdatePhase.Dynamic)
            return;

        if (!isSelected || interactor == null)
            return;

        Vector3 currentDir = GetProjectedLocalDirection();

        float delta = SignedAngleBetween(lastLocalDir, currentDir, GetLocalAxis());

        if (Mathf.Abs(delta) > 0.001f)
        {
            accumulatedAngle += delta;
            ApplyRotation(delta);
            OnCrank?.Invoke(delta);
        }

        lastLocalDir = currentDir;
    }
    
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
/*
        Vector3 euler = Vector3.zero;

        switch (rotationAxis)
        {
            case RotationAxis.X: euler.x = angle; break;
            case RotationAxis.Y: euler.y = angle; break;
            case RotationAxis.Z: euler.z = angle; break;
        }

        pivot.localRotation = Quaternion.Euler(euler);*/
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
        accumulatedAngle += delta;
        OnCrank?.Invoke(delta);
    }
#endif
}
