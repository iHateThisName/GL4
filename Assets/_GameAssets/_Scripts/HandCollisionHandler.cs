using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using System.Collections.Generic;

public class HandCollisionHandler : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private XRBaseInteractor targetInteractor;

    [Header("Layer Settings")]
    [Tooltip("The layer to temporarily assign to objects while they are held.")]
    [SerializeField] private string heldItemLayerName = "HeldItem";

    // A dictionary to remember the exact layer an item was on before we grabbed it
    private Dictionary<GameObject, int> originalLayers = new Dictionary<GameObject, int>();
    private int heldItemLayerIndex;

    private void Awake()
    {
        if (this.targetInteractor == null)
            this.targetInteractor = GetComponentInChildren<XRBaseInteractor>(true);

        this.heldItemLayerIndex = LayerMask.NameToLayer(this.heldItemLayerName);

        if (this.heldItemLayerIndex == -1)
        {
            Debug.LogError($"[HandCollisionHandler] Layer '{this.heldItemLayerName}' does not exist! Please create it in the Tags & Layers menu.");
        }
    }

    private void OnEnable()
    {
        if (this.targetInteractor != null)
        {
            this.targetInteractor.selectEntered.AddListener(OnGrabSwapLayer);
            this.targetInteractor.selectExited.AddListener(OnDropRevertLayer);
        }
    }

    private void OnDisable()
    {
        if (this.targetInteractor != null)
        {
            this.targetInteractor.selectEntered.RemoveListener(OnGrabSwapLayer);
            this.targetInteractor.selectExited.RemoveListener(OnDropRevertLayer);
        }
    }

    public void OnGrabSwapLayer(SelectEnterEventArgs args)
    {
        GameObject item = args.interactableObject.transform.gameObject;

        // Save the item's original layer before we change it
        if (!this.originalLayers.ContainsKey(item))
        {
            this.originalLayers.Add(item, item.layer);
        }

        // Swap the layer of the item and all of its children to 'HeldItem'
        this.SetLayerRecursively(item, this.heldItemLayerIndex);

        Debug.Log($"<color=green>[Layer Swap]</color> Changed {item.name} to {this.heldItemLayerName} layer.");
    }

    public void OnDropRevertLayer(SelectExitEventArgs args)
    {
        GameObject item = args.interactableObject.transform.gameObject;

        // Retrieve the original layer and put it back
        if (this.originalLayers.TryGetValue(item, out int originalLayer))
        {
            this.SetLayerRecursively(item, originalLayer);
            this.originalLayers.Remove(item);

            Debug.Log($"<color=orange>[Layer Swap]</color> Restored {item.name} to layer index {originalLayer}.");
        }
    }

    // Helper method to ensure multi-part items (like models with child colliders) are fully swapped
    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (child != null)
            {
                SetLayerRecursively(child.gameObject, newLayer);
            }
        }
    }
}