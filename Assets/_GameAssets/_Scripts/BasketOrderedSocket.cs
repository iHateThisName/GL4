using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class BasketOrderedSocket : XRSocketInteractor
{
    [Header("Basket Configuration")]
    [Tooltip("If true, it uses the child order in the hierarchy (top to bottom) as the fill order.")]
    [SerializeField] bool m_UseHierarchyOrder = true;

    [Tooltip("The ordered list of slots. The basket fills index 0, then index 1, etc.")]
    [SerializeField] List<Transform> m_OrderedSlots = new List<Transform>();

    readonly Dictionary<IXRInteractable, Transform> m_SelectionMap = new Dictionary<IXRInteractable, Transform>();
    readonly HashSet<Transform> m_OccupiedSlots = new HashSet<Transform>();

    protected override void Awake()
    {
        base.Awake();

        if (m_UseHierarchyOrder && attachTransform != null)
        {
            m_OrderedSlots.Clear();
            foreach (Transform child in attachTransform)
            {
                m_OrderedSlots.Add(child);
            }
        }

        // Essential: Allow the hover mesh to show even if the "main" socket slot is technically occupied
        interactableCantHoverMeshMaterial = interactableHoverMeshMaterial;
    }

    public override Transform GetAttachTransform(IXRInteractable interactable)
    {
        // 1. If already snapped, stay in that slot
        if (m_SelectionMap.TryGetValue(interactable, out var assignedSlot))
            return assignedSlot;

        // 2. Ordered Logic: Always pick the FIRST empty slot in the list
        foreach (var slot in m_OrderedSlots)
        {
            if (!m_OccupiedSlots.Contains(slot))
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
        return base.CanHover(interactable) && m_OccupiedSlots.Count < m_OrderedSlots.Count;
    }

    public override bool CanSelect(IXRSelectInteractable interactable)
    {
        // This is the logic that bypasses the 1-object limit.
        // It returns true if we are already selecting the object OR if the basket isn't full.
        return IsSelecting(interactable) ||
               (m_OccupiedSlots.Count < m_OrderedSlots.Count && !interactable.isSelected);
    }

    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        // We find the target slot BEFORE calling base, because base triggers the snap
        Transform target = GetAttachTransform(args.interactableObject);

        if (target != null && !m_OccupiedSlots.Contains(target))
        {
            m_OccupiedSlots.Add(target);
            m_SelectionMap.Add(args.interactableObject, target);
        }

        base.OnSelectEntering(args);
    }

    protected override void OnSelectExiting(SelectExitEventArgs args)
    {
        if (m_SelectionMap.TryGetValue(args.interactableObject, out var target))
        {
            m_OccupiedSlots.Remove(target);
            m_SelectionMap.Remove(args.interactableObject);
        }
        base.OnSelectExiting(args);
    }

    protected virtual void OnDrawGizmos()
    {
        if (m_OrderedSlots == null || m_OrderedSlots.Count == 0) return;

        for (int i = 0; i < m_OrderedSlots.Count; i++)
        {
            if (m_OrderedSlots[i] == null) continue;

            Gizmos.color = m_OccupiedSlots.Contains(m_OrderedSlots[i]) ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(m_OrderedSlots[i].position, 0.03f);

#if UNITY_EDITOR
            UnityEditor.Handles.Label(m_OrderedSlots[i].position + Vector3.up * 0.05f, "Order: " + i);
#endif
        }
    }
}
