using MonsterSystem;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables; // Added from your script

public class HidingState : MonsterState
{
    [Header("=== Components ===")]
    [SerializeField] private XRGrabInteractable grabInteractable;
    [SerializeField] private UnityEngine.AI.NavMeshAgent navMeshAgent;
    [SerializeField] private MonsterState nextState;

    [Header("=== Bed Reference ===")]
    [SerializeField] private TriggerArea bedTriggerArea;
    [SerializeField] private MonsterState patientState;

    private MonsterSensor dollSensor;

    public override void OnStateEnter()
    {
        // Call base method
        base.OnStateEnter();

        // 1. Prepare physics/pathfinding for grabbing
        if (navMeshAgent != null) navMeshAgent.enabled = false;

        // 2. Enable grabbing
        if (grabInteractable != null)
        {
            grabInteractable.enabled = true;
        }

        // 3. Subscribe to the bed trigger area event
        if (bedTriggerArea != null)
        {
            this.bedTriggerArea.OnTriggerStayed += OnDollDropped;
        }

        // Cache the sensor so we can reset patience when returned to bed
        dollSensor = controller.GetComponent<DollSensor>();
    }

    public override void OnStateExit()
    {
        // Call base method
        base.OnStateExit();

        // Clean up events when leaving this state
        if (bedTriggerArea != null)
        {
            this.bedTriggerArea.OnTriggerStayed -= OnDollDropped;
        }

        // This is crucial: If the timer runs out and she goes Aggressive, 
        // disabling this forces her to drop out of the player's hands!
        if (grabInteractable != null)
        {
            grabInteractable.enabled = false;
        }
    }

    private void OnDollDropped(Collider collider)
    {
        // 1. First, check if the trigger is firing at all
        Debug.Log($"[HidingState] Trigger area detected collider: {collider.gameObject.name}");

        // 2. Check if the object in the trigger area is THIS doll
        if (collider.gameObject != this.gameObject)
        {
            // We don't log here usually to avoid console spam, but if you see other objects
            // triggering the log above, but NOT the doll, check your colliders.
            return;
        }

        Debug.Log("[HidingState] The Doll's collider has been detected in the bed!");

        // 3. Check if the player is still holding the doll. 
        if (grabInteractable != null && grabInteractable.isSelected)
        {
            Debug.Log("[HidingState] The Doll is in the bed, but the player is still holding her.");
            return;
        }

        Debug.Log("[HidingState] The Doll is in the bed AND released! Calling ReturnToBed().");
        ReturnToBed();
    }

    private void ReturnToBed()
    {
        Debug.Log("[HidingState] Executing ReturnToBed().");

        if (bedTriggerArea != null)
        {
            this.bedTriggerArea.OnTriggerStayed -= OnDollDropped;
        }

        if (grabInteractable != null) grabInteractable.enabled = false;

        // Use the updated ResetTimer method
        var petSensor = this.controller.GetSensor<DollSensor>();
        if (petSensor != null)
        {
            Debug.Log("[HidingState] Resetting the tension timer.");
            petSensor.ResetTimer();
        }
        else
        {
            Debug.LogError("[HidingState] Could not find DollSensor to reset timer!");
        }

        if (this.nextState != null)
        {
            Debug.Log($"[HidingState] Transitioning to next state: {this.nextState.name}");
            RequestTransition(this.nextState);
        }
        else
        {
            Debug.LogError("[HidingState] nextState is completely NULL in the inspector!");
        }
    }
}