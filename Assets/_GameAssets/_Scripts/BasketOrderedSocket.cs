using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class BasketOrderedSocket : XRSocketInteractor
{
    [Header("Basket Configuration")]
    [Tooltip("If true, it uses the child order in the hierarchy (top to bottom) as the fill order.")]
    [SerializeField] bool useHierarchyOrder = true;

    [Tooltip("The ordered list of slots. The basket fills index 0, then index 1, etc.")]
    [SerializeField] List<Transform> orderedSlots = new List<Transform>();

    readonly Dictionary<IXRInteractable, Transform> SELECTION_MAP = new Dictionary<IXRInteractable, Transform>();
    readonly HashSet<Transform> OCCUPIED_SLOTS = new HashSet<Transform>();

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

        // Essential: Allow the hover mesh to show even if the "main" socket slot is technically occupied
        this.interactableCantHoverMeshMaterial = this.interactableHoverMeshMaterial;
    }

    public override Transform GetAttachTransform(IXRInteractable interactable)
    {
        // 1. If already snapped, stay in that slot
        if (this.SELECTION_MAP.TryGetValue(interactable, out var assignedSlot))
            return assignedSlot;

        // 2. Ordered Logic: Always pick the FIRST empty slot in the list
        foreach (var slot in this.orderedSlots)
        {
            if (!this.OCCUPIED_SLOTS.Contains(slot))
            {
                return slot;
            }
        }

        // Fallback to base if full
        return base.attachTransform;
    }

    // --- Critical Overrides to make Multi-Snap work ---

    public override bool CanHover(IXRHoverInteractable interactable)
    {
        // Check if there is any room left in the basket
        return base.CanHover(interactable) && this.OCCUPIED_SLOTS.Count < this.orderedSlots.Count;
    }

    public override bool CanSelect(IXRSelectInteractable interactable)
    {
        // This is the logic that bypasses the 1-object limit.
        // It returns true if we are already selecting the object OR if the basket isn't full.
        return IsSelecting(interactable) ||
               (this.OCCUPIED_SLOTS.Count < this.orderedSlots.Count && !interactable.isSelected);
    }

    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        // We find the target slot BEFORE calling base, because base triggers the snap
        Transform target = GetAttachTransform(args.interactableObject);

        if (target != null && !this.OCCUPIED_SLOTS.Contains(target))
        {
            this.OCCUPIED_SLOTS.Add(target);
            this.SELECTION_MAP.Add(args.interactableObject, target);
        }

        base.OnSelectEntering(args);
    }

    protected override void OnSelectExiting(SelectExitEventArgs args)
    {
        if (this.SELECTION_MAP.TryGetValue(args.interactableObject, out var target))
        {
            this.OCCUPIED_SLOTS.Remove(target);
            this.SELECTION_MAP.Remove(args.interactableObject);
        }
        base.OnSelectExiting(args);
    }

    protected virtual void OnDrawGizmos()
    {
        if (this.orderedSlots == null || this.orderedSlots.Count == 0) return;

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
}
