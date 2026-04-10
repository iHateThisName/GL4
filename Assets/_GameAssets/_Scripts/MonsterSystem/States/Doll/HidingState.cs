using MonsterSystem;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class HidingState : MonsterState
{
    [Header("=== Components ===")]
    [SerializeField] private XRGrabInteractable grabInteractable;
    [SerializeField] private UnityEngine.AI.NavMeshAgent navMeshAgent;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private MonsterState nextState;

    [Header("=== Bed Reference ===")]
    [SerializeField] private SO_TransformRef triggerReferanceObject;

    [Header("=== Doll Detection ===")]
    [Tooltip("Drag the GameObject containing the Doll's actual collider here.")]
    [SerializeField] private Collider dollCollider;

    private TriggerArea bedTriggerArea;

    public override void Initialize(MonsterController owningController)
    {
        base.Initialize(owningController);
        if (bedTriggerArea != null)
        {
            this.bedTriggerArea = this.triggerReferanceObject.Value.GetComponent<TriggerArea>();
        }
    }
    public override void OnStateEnter()
    {
        base.OnStateEnter();

        // 1. Turn OFF pathfinding
        if (navMeshAgent != null) navMeshAgent.enabled = false;

        // 2. Turn ON physics explicitly so she falls/can be grabbed
        if (rb != null) rb.isKinematic = false;

        // 3. Enable VR grabbing
        if (grabInteractable != null)
        {
            grabInteractable.enabled = true;
        }

        // 4. Listen for the bed
        if (bedTriggerArea != null)
        {
            this.bedTriggerArea.OnTriggerStayed += OnDollDropped;
        }
    }

    public override void OnStateExit()
    {
        base.OnStateExit();

        if (bedTriggerArea != null)
        {
            this.bedTriggerArea.OnTriggerStayed -= OnDollDropped;
        }

        // Force drop her if the timer runs out and she goes Aggressive
        if (grabInteractable != null)
        {
            grabInteractable.enabled = false;
        }

        // Notice we removed the rb.isKinematic = true here!
        // We will let the AggressiveState or PatientState handle their own physics needs.
    }

    private void OnDollDropped(Collider collider)
    {
        if (dollCollider != null && collider != dollCollider) return;
        if (grabInteractable != null && grabInteractable.isSelected) return;

        ReturnToBed();
    }

    private void ReturnToBed()
    {
        if (bedTriggerArea != null)
        {
            this.bedTriggerArea.OnTriggerStayed -= OnDollDropped;
        }

        if (grabInteractable != null) grabInteractable.enabled = false;

        var petSensor = this.controller.GetSensor<DollSensor>();
        if (petSensor != null)
        {
            petSensor.ResetTimer();
        }

        if (this.nextState != null)
        {
            RequestTransition(this.nextState);
        }
    }
}