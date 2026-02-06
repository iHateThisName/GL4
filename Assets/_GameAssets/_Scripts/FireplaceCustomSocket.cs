using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class FireplaceCustomSocket : XRSocketInteractor
{
    [Header("Basket Configuration")]
    [Tooltip("If true, it uses the child order in the hierarchy (top to bottom) as the fill order.")]
    [SerializeField] private bool useHierarchyOrder = true;

    [Tooltip("The ordered list of slots. The basket fills index 0, then index 1, etc.")]
    [SerializeField] private List<Transform> orderedSlots = new List<Transform>();

    [Header("Snap Settings")]
    [Tooltip("How close (in meters) the object must be to the assigned slot to snap.")]
    [SerializeField] private float snapDistance = 0.1f;

    private readonly Dictionary<IXRInteractable, Transform> SELECTION_MAP = new Dictionary<IXRInteractable, Transform>();
    private readonly HashSet<Transform> OCCUPIED_SLOTS = new HashSet<Transform>();

    #region Unity Lifecycle
    // Sets up the slot list from children and prepares hover materials
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
    // Determines which specific slot transform an object should snap to
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

    // Validates if an object is close enough to its assigned slot to show a preview
    public override bool CanHover(IXRHoverInteractable interactable)
    {
        Transform target = this.GetAttachTransform(interactable);
        if (target == null || target == this.attachTransform) return false;

        float distance = Vector3.Distance(interactable.GetAttachTransform(this).position, target.position);

        return base.CanHover(interactable) &&
               this.OCCUPIED_SLOTS.Count < this.orderedSlots.Count &&
               distance <= this.snapDistance;
    }

    // Logic for allowing the actual snap to occur based on distance and capacity
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

    // Registers the object to a slot when it enters the socket
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

    // Clears the slot occupancy when an object is removed
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
    // Draws visual range spheres + raycast of rotation and slot labels in the Scene view
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

            Gizmos.color = isOccupied ? Color.red : Color.cyan;
            Gizmos.DrawRay(slot.position, slot.forward * 0.1f);
            Gizmos.DrawWireSphere(slot.position, 0.01f);

#if UNITY_EDITOR
            string label = isOccupied ? $"[{i}] Occupied" : $"[{i}] Next Available";
            UnityEditor.Handles.Label(slot.position + Vector3.up * (this.snapDistance + 0.02f), label);
#endif
        }
    }
    #endregion
}