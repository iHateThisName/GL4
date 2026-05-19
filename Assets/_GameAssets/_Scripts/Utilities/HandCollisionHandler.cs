using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

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

    public event System.Action<EnumGrabItems> OnGrabItem;
    public event System.Action<EnumGrabItems> OnReleaseItem;
    private EnumGrabItems currentGrabedItem;

    private void Awake()
    {
        if (this.targetInteractor == null)
        {
            this.targetInteractor = this.GetComponentInChildren<XRBaseInteractor>(true);
        }

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
            this.targetInteractor.selectEntered.AddListener(this.OnGrabSwapLayer);
            this.targetInteractor.selectExited.AddListener(this.OnDropRevertLayer);
        }
    }

    private void OnDisable()
    {
        if (this.targetInteractor != null)
        {
            this.targetInteractor.selectEntered.RemoveListener(this.OnGrabSwapLayer);
            this.targetInteractor.selectExited.RemoveListener(this.OnDropRevertLayer);
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

        // You can pass an enum or any identifier to specify which item was grabbed
        this.currentGrabedItem = GetItemType(item);
        this.OnGrabItem?.Invoke(this.currentGrabedItem);

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

            this.OnReleaseItem?.Invoke(this.currentGrabedItem);
        }
    }

    // Helper method to ensure multi-part items (like models with child colliders) are fully swapped
    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null)
        {
            return;
        }

        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            if (child != null)
            {
                this.SetLayerRecursively(child.gameObject, newLayer);
            }
        }
    }

    private EnumGrabItems GetItemType(GameObject item) {
        if (item.CompareTag("Axe")) {
            return EnumGrabItems.Axe;
        } else if (item.CompareTag("Flashlight")) {
            return EnumGrabItems.Flashlight;
        } else if (item.CompareTag("Glass")) {
            return EnumGrabItems.Glass;
        } else if (item.CompareTag("Food")) {
            return EnumGrabItems.Cans;
        }
        return EnumGrabItems.Default;
    }
    public enum EnumGrabItems { Default = 4, Axe = 0, Flashlight = 1, Glass = 2, Cans = 3 }
}