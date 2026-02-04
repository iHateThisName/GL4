using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class BasketCustomSocket : XRSocketInteractor
{
    [Header("Basket Configuration")]
    [SerializeField] private bool useHierarchyOrder = true;
    [SerializeField] private List<Transform> orderedSlots = new List<Transform>();

    [Header("Snap Settings")]
    [SerializeField] private float snapDistance = 0.1f;

    private readonly Dictionary<IXRInteractable, Transform> SELECTION_MAP = new Dictionary<IXRInteractable, Transform>();
    private readonly HashSet<Transform> OCCUPIED_SLOTS = new HashSet<Transform>();

    #region Unity Lifecycle
    // Initializes slots based on child hierarchy order
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
    // Finds the next available slot in index order
    public override Transform GetAttachTransform(IXRInteractable interactable)
    {
        if (this.SELECTION_MAP.TryGetValue(interactable, out Transform assignedSlot))
            return assignedSlot;

        foreach (Transform slot in this.orderedSlots)
        {
            if (!this.OCCUPIED_SLOTS.Contains(slot))
                return slot;
        }
        return base.attachTransform;
    }

    // Prevents hover state if object is outside range or basket is full
    public override bool CanHover(IXRHoverInteractable interactable)
    {
        Transform target = this.GetAttachTransform(interactable);
        if (target == null || target == this.attachTransform) return false;

        float distance = Vector3.Distance(interactable.GetAttachTransform(this).position, target.position);

        return base.CanHover(interactable) &&
               this.OCCUPIED_SLOTS.Count < this.orderedSlots.Count &&
               distance <= this.snapDistance;
    }

    // Decides if an object can actually snap into the next slot
    public override bool CanSelect(IXRSelectInteractable interactable)
    {
        Transform target = this.GetAttachTransform(interactable);
        if (target == null || target == this.attachTransform) return false;

        float distance = Vector3.Distance(interactable.GetAttachTransform(this).position, target.position);

        return this.IsSelecting(interactable) ||
               (this.OCCUPIED_SLOTS.Count < this.orderedSlots.Count &&
                !interactable.isSelected &&
                distance <= this.snapDistance);
    }

    // Handles logic for when an object is successfully placed
    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        Transform target = this.GetAttachTransform(args.interactableObject);
        if (target != null && target != this.attachTransform && !this.OCCUPIED_SLOTS.Contains(target))
        {
            this.OCCUPIED_SLOTS.Add(target);
            this.SELECTION_MAP.Add(args.interactableObject, target);
        }
        base.OnSelectEntering(args);
    }

    // Handles logic for when an object is removed from the basket
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
    // Draws range bubbles and slot indexes in the Unity Editor scene view
    protected virtual void OnDrawGizmos()
    {
        if (this.orderedSlots == null || this.orderedSlots.Count == 0) return;
        for (int i = 0; i < this.orderedSlots.Count; i++)
        {
            Transform slot = this.orderedSlots[i];
            if (slot == null) continue;
            bool isOccupied = this.OCCUPIED_SLOTS.Contains(slot);
            Gizmos.color = isOccupied ? new Color(1, 0, 0, 0.2f) : new Color(0, 1, 1, 0.3f);
            Gizmos.DrawSphere(slot.position, this.snapDistance);
        }
    }
    #endregion
}