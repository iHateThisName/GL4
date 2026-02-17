using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class CrankRotationInteractable : MonoBehaviour
{
    [SerializeField] private XRSimpleInteractable interactable;
    
    [SerializeField] private Transform pivot;        // rotates
    [SerializeField] private Transform visual;       // optional, purely visual
    [SerializeField] private RotatorInteractable.RotationAxis rotationAxis = RotatorInteractable.RotationAxis.Y;
    [SerializeField] private bool isRotationClamped;
    [SerializeField] private float minAngle = -90f;
    [SerializeField] private float maxAngle = 90f;
    
    private IXRSelectInteractor interactor;
    private Vector3 lastLocalDir;
    private float accumulatedAngle;
    private bool hasInput;
    
    public System.Action<float> OnCrank;
    
    private void OnEnable()
    {
        interactable.selectEntered.AddListener(StartCranking);
        interactable.selectExited.AddListener(EndCranking);
    }

    private void OnDisable()
    {
        interactable.selectEntered.AddListener(StartCranking);
        interactable.selectExited.AddListener(EndCranking);
    }
    
    void Update()
    {
        if (interactor == null) return;

        Transform attach = interactor.GetAttachTransform(this.interactable);

        this.UpdateInput(attach.position);
    }

    private void StartCranking(SelectEnterEventArgs args)
    {
        if (interactor != null) return; 
        interactor = args.interactorObject;
        Transform attach = interactor.GetAttachTransform(interactable);
        lastLocalDir = GetProjectedWorldDirection(attach.position);//GetProjectedLocalDirection(attach.position);
        hasInput = true;
    }

    private void EndCranking(SelectExitEventArgs args)
    {
        if (args.interactorObject != interactor) return;
        interactor = null;
        hasInput = false;
    }

    private void UpdateInput(Vector3 position)
    {
        if (!hasInput) return;

        Vector3 currentDir = GetProjectedWorldDirection(position);
        float delta = SignedAngleBetween(lastLocalDir, currentDir, GetWorldAxis());

        if (Mathf.Abs(delta) > 0.001f)
        {
            accumulatedAngle += delta;
            ApplyRotation(delta);
            OnCrank?.Invoke(delta);
        }

        lastLocalDir = currentDir;
    }
    
    Vector3 GetProjectedWorldDirection(Vector3 worldPos)
    {
        Vector3 worldDir = worldPos - pivot.position;

        Vector3 axis = GetWorldAxis();

        // Project onto rotation plane
        worldDir -= Vector3.Dot(worldDir, axis) * axis;

        return worldDir.normalized;
    }
    
    Vector3 GetProjectedLocalDirection(Vector3 worldPos)
    {
        Vector3 worldDir = worldPos - pivot.position;
        Vector3 localDir = pivot.parent.InverseTransformDirection(worldDir);

        Vector3 axis = GetLocalAxis();
        localDir -= Vector3.Dot(localDir, axis) * axis;

        return localDir.normalized;
    }
    
    private Vector3 GetProjectedLocalDirection()
    {
        Vector3 worldDir =
            interactor.GetAttachTransform(this.interactable).position - pivot.position;

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
            RotatorInteractable.RotationAxis.X => Vector3.right,
            RotatorInteractable.RotationAxis.Y => Vector3.up,
            RotatorInteractable.RotationAxis.Z => Vector3.forward,
            _ => Vector3.forward
        };
    }
    
    Vector3 GetWorldAxis()
    {
        return rotationAxis switch
        {
            RotatorInteractable.RotationAxis.X => pivot.right,
            RotatorInteractable.RotationAxis.Y => pivot.up,
            RotatorInteractable.RotationAxis.Z => pivot.forward,
            _ => pivot.forward
        };
    }

    void ApplyRotation(float deltaAngle)
    {
        if (!pivot) return;
        Vector3 axis = GetWorldAxis();

        pivot.rotation = Quaternion.AngleAxis(deltaAngle, axis) * pivot.rotation;
    }
}
