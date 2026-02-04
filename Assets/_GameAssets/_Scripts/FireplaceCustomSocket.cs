using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;


public class FireplaceCustomSocket : XRSocketInteractor
{
    [Header("Basket Configuration")]
    [Tooltip("If true, it uses the child order in the hierarchy (top to bottom) as the fill order.")]
    [SerializeField] bool useHierarchyOrder = true;

    [Tooltip("The ordered list of slots. The basket fills index 0, then index 1, etc.")]
    [SerializeField] List<Transform> orderedSlots = new List<Transform>();

    [Header("Snap Settings")]
    [Tooltip("How close (in meters) the object must be to the assigned slot to snap.")]
    [SerializeField] float m_SnapDistance = 0.1f;

    readonly Dictionary<IXRInteractable, Transform> SELECTION_MAP = new Dictionary<IXRInteractable, Transform>();
    readonly HashSet<Transform> OCCUPIED_SLOTS = new HashSet<Transform>();

    protected override void Awake()
    {
        base.Awake();

        // 1. Setup Slots
        if (this.useHierarchyOrder && this.attachTransform != null)
        {
            this.orderedSlots.Clear();
            foreach (Transform child in this.attachTransform)
            {
                this.orderedSlots.Add(child);
            }
        }

        // 2. Ensure we can see the hover mesh even if technically "full"
        this.interactableCantHoverMeshMaterial = this.interactableHoverMeshMaterial;
    }

    public override Transform GetAttachTransform(IXRInteractable interactable)
    {
        // 1. If already snapped, stay in that slot
        if (this.SELECTION_MAP.TryGetValue(interactable, out var assignedSlot))
            return assignedSlot;

        // 2. Ordered Logic: Find the FIRST empty slot
        foreach (var slot in this.orderedSlots)
        {
            if (!this.OCCUPIED_SLOTS.Contains(slot))
            {
                return slot;
            }
        }

        return base.attachTransform;
    }

    public override bool CanHover(IXRHoverInteractable interactable)
    {
        Transform target = GetAttachTransform(interactable);
        if (target == null || target == base.attachTransform) return false;

        // Calculate distance to the slot the script is CURRENTLY assigning
        float dist = Vector3.Distance(interactable.GetAttachTransform(this).position, target.position);

        return base.CanHover(interactable) &&
               this.OCCUPIED_SLOTS.Count < this.orderedSlots.Count &&
               dist <= m_SnapDistance;
    }

    public override bool CanSelect(IXRSelectInteractable interactable)
    {
        Transform target = GetAttachTransform(interactable);
        if (target == null || target == base.attachTransform) return false;

        float dist = Vector3.Distance(interactable.GetAttachTransform(this).position, target.position);

        return IsSelecting(interactable) ||
               (this.OCCUPIED_SLOTS.Count < this.orderedSlots.Count &&
                !interactable.isSelected &&
                dist <= m_SnapDistance);
    }

    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        // IMPORTANT: Calculate target before calling base.OnSelectEntering
        Transform target = GetAttachTransform(args.interactableObject);

        if (target != null && target != base.attachTransform && !this.OCCUPIED_SLOTS.Contains(target))
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
            Transform slot = this.orderedSlots[i];
            if (slot == null) continue;

            bool isOccupied = this.OCCUPIED_SLOTS.Contains(slot);

            // 1. Draw Snap Range (Cyan for free, Red for occupied)
            Gizmos.color = isOccupied ? new Color(1, 0, 0, 0.2f) : new Color(0, 1, 1, 0.3f);
            Gizmos.DrawSphere(slot.position, m_SnapDistance);

            // 2. Draw Rotation Arrow
            Gizmos.color = isOccupied ? Color.red : Color.cyan;
            Gizmos.DrawRay(slot.position, slot.forward * 0.1f);
            Gizmos.DrawWireSphere(slot.position, 0.01f);

#if UNITY_EDITOR
            // 3. Draw Socket Numbers
            string label = isOccupied ? $"[{i}] Occupied" : $"[{i}] Next Available";
            UnityEditor.Handles.Label(slot.position + Vector3.up * (m_SnapDistance + 0.02f), label);
#endif
        }
    }
}
