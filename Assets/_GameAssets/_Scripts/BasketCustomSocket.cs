using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class BasketCustomSocket : XRSocketInteractor
{
    [Header("Basket Configuration")]
    [Tooltip("If true, it uses the child order in the hierarchy (top to bottom) as the fill order.")]
    [SerializeField] private bool useHierarchyOrder = true;

    [Tooltip("The ordered list of slots. The basket fills index 0, then index 1, etc.")]
    [SerializeField] private List<Transform> orderedSlots = new List<Transform>();

    private readonly Dictionary<IXRInteractable, Transform> SELECTION_MAP = new Dictionary<IXRInteractable, Transform>();
    private readonly HashSet<Transform> OCCUPIED_SLOTS = new HashSet<Transform>();

    #region Unity Lifecycle
    // Initializes the slot list from children and prepares hover visuals
    protected override void Awake()
    {
        base.Awake();

        if (this.useHierarchyOrder && this.attachTransform != null)
        {
            this.orderedSlots.Clear();
            foreach (Transform child in this.attachTransform)
            {
                this.orderedSlots.Add(child);
            }
        }

        this.interactableCantHoverMeshMaterial = this.interactableHoverMeshMaterial;
    }
    #endregion

    #region XRI Overrides
    // Identifies the correct slot transform for a specific object
    public override Transform GetAttachTransform(IXRInteractable interactable)
    {
        if (this.SELECTION_MAP.TryGetValue(interactable, out Transform assignedSlot))
        {
            return assignedSlot;
        }

        foreach (Transform slot in this.orderedSlots)
        {
            if (!this.OCCUPIED_SLOTS.Contains(slot))
            {
                return slot;
            }
        }

        return base.attachTransform;
    }

    // Determines if an object can trigger a hover state based on capacity
    public override bool CanHover(IXRHoverInteractable interactable)
    {
        return base.CanHover(interactable) && this.OCCUPIED_SLOTS.Count < this.orderedSlots.Count;
    }

    // Determines if an object can be selected/snapped based on capacity
    public override bool CanSelect(IXRSelectInteractable interactable)
    {
        return this.IsSelecting(interactable) ||
               (this.OCCUPIED_SLOTS.Count < this.orderedSlots.Count && !interactable.isSelected);
    }

    // Logic for assigning an object to a specific slot upon entry
    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        Transform target = this.GetAttachTransform(args.interactableObject);

        if (target != null && !this.OCCUPIED_SLOTS.Contains(target))
        {
            this.OCCUPIED_SLOTS.Add(target);
            this.SELECTION_MAP.Add(args.interactableObject, target);
        }

        base.OnSelectEntering(args);
    }

    // Logic for freeing up a slot when an object is removed
    protected override void OnSelectExiting(SelectExitEventArgs args)
    {
        if (this.SELECTION_MAP.TryGetValue(args.interactableObject, out Transform target))
        {
            this.OCCUPIED_SLOTS.Remove(target);
            this.SELECTION_MAP.Remove(args.interactableObject);
        }

        base.OnSelectExiting(args);
    }
    #endregion

    #region Editor Visualization
    // Draws editor-only gizmos to visualize slot occupancy and order
    protected virtual void OnDrawGizmos()
    {
        if (this.orderedSlots == null || this.orderedSlots.Count == 0)
        {
            return;
        }

        for (int i = 0; i < this.orderedSlots.Count; i++)
        {
            if (this.orderedSlots[i] == null) continue;

            Gizmos.color = this.OCCUPIED_SLOTS.Contains(this.orderedSlots[i]) ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(this.orderedSlots[i].position, 0.03f);

#if UNITY_EDITOR
            UnityEditor.Handles.Label(this.orderedSlots[i].position + Vector3.up * 0.05f, "Order: " + i);
#endif
        }
    }
    #endregion
}