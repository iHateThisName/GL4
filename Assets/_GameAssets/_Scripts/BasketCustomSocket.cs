using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class BasketCustomSocket : XRSocketInteractor
{
    [Header("Basket Configuration")]
    [Tooltip("If true, it uses the child order in the hierarchy as the fill order.")]
    [SerializeField] private bool useHierarchyOrder = true;

    [Tooltip("The ordered list of slots.")]
    [SerializeField] private List<Transform> orderedSlots = new List<Transform>();

    [Header("Audio Settings")]
    [SerializeField] private AudioClip basketEntrySFX;

    private readonly Dictionary<IXRInteractable, Transform> selectionMap = new Dictionary<IXRInteractable, Transform>();
    private readonly HashSet<Transform> occupiedSlots = new HashSet<Transform>();

    #region Unity Lifecycle
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
    public override Transform GetAttachTransform(IXRInteractable interactable)
    {
        if (this.selectionMap.TryGetValue(interactable, out Transform assignedSlot))
        {
            return assignedSlot;
        }

        foreach (Transform slot in this.orderedSlots)
        {
            if (!this.occupiedSlots.Contains(slot))
            {
                return slot;
            }
        }

        return base.attachTransform;
    }

    public override bool CanHover(IXRHoverInteractable interactable)
    {
        return base.CanHover(interactable) && this.occupiedSlots.Count < this.orderedSlots.Count;
    }

    public override bool CanSelect(IXRSelectInteractable interactable)
    {
        return this.IsSelecting(interactable) ||
               (this.occupiedSlots.Count < this.orderedSlots.Count && !interactable.isSelected);
    }

    protected override void OnSelectEntering(SelectEnterEventArgs args)
    {
        Transform target = this.GetAttachTransform(args.interactableObject);

        if (target != null && !this.occupiedSlots.Contains(target))
        {
            this.occupiedSlots.Add(target);
            this.selectionMap.Add(args.interactableObject, target);

            //Audio
            PlayEntrySound();
        }

        base.OnSelectEntering(args);
    }

    protected override void OnSelectExiting(SelectExitEventArgs args)
    {

        if (this.selectionMap.TryGetValue(args.interactableObject, out Transform target))
        {
            this.occupiedSlots.Remove(target);
            this.selectionMap.Remove(args.interactableObject);
        }

        base.OnSelectExiting(args);
    }
    #endregion

    #region Editor Visualization
    protected virtual void OnDrawGizmos()
    {
        if (this.orderedSlots == null || this.orderedSlots.Count == 0) return;

        for (int i = 0; i < this.orderedSlots.Count; i++)
        {
            if (this.orderedSlots[i] == null) continue;

            Gizmos.color = this.occupiedSlots.Contains(this.orderedSlots[i]) ? Color.red : Color.yellow;
            Gizmos.DrawWireSphere(this.orderedSlots[i].position, 0.03f);
        }
    }
    #endregion

    private void PlayEntrySound()
    {
        if (basketEntrySFX != null && SoundEffectManager.Instance != null)
        {
            // We use the basket's transform so the sound is localized to the basket
            SoundEffectManager.Instance.PlaySoundFXClip(basketEntrySFX, transform, 1f);
        }
    }
}