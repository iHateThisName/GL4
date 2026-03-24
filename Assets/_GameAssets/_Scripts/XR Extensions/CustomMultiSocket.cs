using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

/// <summary>
/// A Multi-Socket Interactor that allows holding multiple objects by using a list of Transforms.
/// It bypasses the standard XRSocketInteractor limit of one item.
/// </summary>

public class CustomMultiSocket : XRSocketInteractor
{
    [Header("Slot Configuration")]
    [SerializeField] bool m_AutoFindChildSlots = true;
    [SerializeField] List<Transform> m_ManualSlots = new List<Transform>();

    [Header("Snap Settings")]
    [Tooltip("How close (in meters) the object must be to a slot to snap. High values = greedy, Low values = precise.")]
    [SerializeField] float m_SnapDistance = 0.05f; // 5cm is a good starting point for precision

    readonly Dictionary<IXRInteractable, Transform> m_SelectionMap = new Dictionary<IXRInteractable, Transform>();
    readonly HashSet<Transform> m_OccupiedSlots = new HashSet<Transform>();

    protected override void Awake()
    {
        base.Awake();

        if (m_AutoFindChildSlots && attachTransform != null)
        {
            foreach (Transform child in attachTransform)
            {
                if (!m_ManualSlots.Contains(child))
                    m_ManualSlots.Add(child);
            }
        }

        if (m_ManualSlots.Count == 0 && attachTransform != null)
            m_ManualSlots.Add(attachTransform);

        interactableCantHoverMeshMaterial = interactableHoverMeshMaterial;
    }

    public override Transform GetAttachTransform(IXRInteractable interactable)
    {
        if (m_SelectionMap.TryGetValue(interactable, out var assignedSlot))
            return assignedSlot;

        Transform closestSlot = null;
        float minDistance = float.MaxValue;
        Vector3 interactablePos = interactable.GetAttachTransform(this).position;

        foreach (var slot in m_ManualSlots)
        {
            if (m_OccupiedSlots.Contains(slot)) continue;

            float dist = Vector3.Distance(interactablePos, slot.position);
            if (dist < minDistance)
            {
                minDistance = dist;
                closestSlot = slot;
            }
        }

        return closestSlot; // Return null if no slot is found (base class handles this)
    }

    public override bool CanHover(IXRHoverInteractable interactable)
    {
        Transform target = GetAttachTransform(interactable);
        if (target == null) return false;

        // NEW: Check if we are close enough to the specific slot to show the hover ghost
        float dist = Vector3.Distance(interactable.GetAttachTransform(this).position, target.position);

        return base.CanHover(interactable) && !m_OccupiedSlots.Contains(target) && dist <= m_SnapDistance;
    }

    public override bool CanSelect(IXRSelectInteractable interactable)
    {
        Transform target = GetAttachTransform(interactable);
        if (target == null) return false;

        // NEW: Check distance again for the actual selection/snap
        float dist = Vector3.Distance(interactable.GetAttachTransform(this).position, target.position);

        return IsSelecting(interactable) ||
               (m_OccupiedSlots.Count < m_ManualSlots.Count &&
                !interactable.isSelected &&
                !m_OccupiedSlots.Contains(target) &&
                dist <= m_SnapDistance);
    }

    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        base.OnSelectEntering(args);
        Transform target = GetAttachTransform(args.interactableObject);

        if (target != null && !m_OccupiedSlots.Contains(target))
        {
            m_OccupiedSlots.Add(target);
            m_SelectionMap.Add(args.interactableObject, target);
        }
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
        if (m_ManualSlots == null) return;

        foreach (var slot in m_ManualSlots)
        {
            if (slot == null) continue;

            // Draw a sphere representing the "Snap Zone" for each slot
            Gizmos.color = new Color(0, 1, 1, 0.3f);
            Gizmos.DrawWireSphere(slot.position, m_SnapDistance);

            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(slot.position, slot.forward * 0.1f);
        }
    }
}