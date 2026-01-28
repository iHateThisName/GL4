using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class MultiSocketInteractor : MonoBehaviour
{
    [Header("Socket Points")]
    [SerializeField] private List<Transform> socketPoints = new List<Transform>();
    
    [Header("Preview Settings")]
    [SerializeField] private GameObject previewPrefab;
    [SerializeField] private Material previewMaterial;
    [SerializeField] private bool showPreviewWhenEmpty = true;
    
    private class SocketSlot
    {
        public Transform attachPoint;
        public UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable occupyingInteractable;
        public GameObject previewInstance;
        public Rigidbody originalRigidbody;
        public bool wasKinematic;
        public bool wasUsingGravity;
        
        public bool IsAvailable => occupyingInteractable == null;
    }
    
    private List<SocketSlot> slots = new List<SocketSlot>();
    private Dictionary<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable, SocketSlot> interactableToSlotMap = new Dictionary<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable, SocketSlot>();
    private HashSet<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable> trackedInteractables = new HashSet<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
    
    private void Awake()
    {
        InitializeSlots();
    }
    
    private void InitializeSlots()
    {
        foreach (var socketTransform in socketPoints)
        {
            if (socketTransform == null)
            {
                Debug.LogWarning("Socket point is null, skipping.");
                continue;
            }
            
            SocketSlot slot = new SocketSlot { attachPoint = socketTransform };
            
            // Create preview if prefab is provided
            if (previewPrefab != null)
            {
                CreatePreview(slot);
            }
            
            slots.Add(slot);
        }
    }
    
    private void CreatePreview(SocketSlot slot)
    {
        slot.previewInstance = Instantiate(previewPrefab, slot.attachPoint);
        slot.previewInstance.name = $"Preview_{slot.attachPoint.name}";
        slot.previewInstance.transform.localPosition = Vector3.zero;
        slot.previewInstance.transform.localRotation = Quaternion.identity;
        
        // Apply preview material if provided
        if (previewMaterial != null)
        {
            Renderer[] renderers = slot.previewInstance.GetComponentsInChildren<Renderer>();
            foreach (var renderer in renderers)
            {
                Material[] mats = new Material[renderer.materials.Length];
                for (int i = 0; i < mats.Length; i++)
                {
                    mats[i] = previewMaterial;
                }
                renderer.materials = mats;
            }
        }
        
        // Disable colliders on preview
        Collider[] colliders = slot.previewInstance.GetComponentsInChildren<Collider>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }
        
        // Disable any interactables on preview
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable[] interactables = slot.previewInstance.GetComponentsInChildren<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        foreach (var interactable in interactables)
        {
            interactable.enabled = false;
        }
        
        slot.previewInstance.SetActive(showPreviewWhenEmpty);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable interactable = other.GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (interactable != null && !trackedInteractables.Contains(interactable))
        {
            trackedInteractables.Add(interactable);
            
            if (!interactable.isSelected)
            {
                TrySnapToSlot(interactable);
            }
            else
            {
                // Show preview on closest available slot when hovering with held item
                ShowClosestPreview(interactable);
            }
        }
    }
    
    private void OnTriggerStay(Collider other)
    {
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable interactable = other.GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (interactable != null)
        {
            // If item was just released, snap it
            if (!interactable.isSelected && !interactableToSlotMap.ContainsKey(interactable))
            {
                TrySnapToSlot(interactable);
            }
            // If still holding, update preview
            else if (interactable.isSelected && !interactableToSlotMap.ContainsKey(interactable))
            {
                ShowClosestPreview(interactable);
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable interactable = other.GetComponentInParent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable>();
        if (interactable != null)
        {
            trackedInteractables.Remove(interactable);
            
            // If being grabbed out, release the slot
            if (interactableToSlotMap.TryGetValue(interactable, out SocketSlot slot))
            {
                if (interactable.isSelected)
                {
                    ReleaseSlot(interactable, slot);
                }
            }
            else
            {
                // Hide all previews when exiting without snapping
                HideAllPreviews();
            }
        }
    }
    
    private void ShowClosestPreview(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable interactable)
    {
        SocketSlot closestSlot = FindClosestAvailableSlot(interactable.transform.position);
        
        // Hide all previews first
        foreach (var slot in slots)
        {
            if (slot.previewInstance != null && slot.IsAvailable)
            {
                slot.previewInstance.SetActive(false);
            }
        }
        
        // Show preview on closest available slot
        if (closestSlot != null && closestSlot.previewInstance != null)
        {
            closestSlot.previewInstance.SetActive(true);
        }
    }
    
    private void HideAllPreviews()
    {
        foreach (var slot in slots)
        {
            if (slot.previewInstance != null && slot.IsAvailable)
            {
                slot.previewInstance.SetActive(showPreviewWhenEmpty);
            }
        }
    }
    
    private void TrySnapToSlot(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable interactable)
    {
        // Find the closest available slot
        SocketSlot closestSlot = FindClosestAvailableSlot(interactable.transform.position);
        
        if (closestSlot != null)
        {
            AssignToSlot(interactable, closestSlot);
        }
    }
    
    private SocketSlot FindClosestAvailableSlot(Vector3 position)
    {
        SocketSlot closestSlot = null;
        float closestDistance = float.MaxValue;
        
        foreach (var slot in slots)
        {
            if (!slot.IsAvailable) continue;
            
            float distance = Vector3.Distance(slot.attachPoint.position, position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestSlot = slot;
            }
        }
        
        return closestSlot;
    }
    
    private void AssignToSlot(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable interactable, SocketSlot slot)
    {
        // Mark slot as occupied
        slot.occupyingInteractable = interactable;
        interactableToSlotMap[interactable] = slot;
        
        // Hide preview
        if (slot.previewInstance != null)
        {
            slot.previewInstance.SetActive(false);
        }
        
        // Store rigidbody state and disable physics (like XRSocketInteractor does)
        Rigidbody rb = interactable.GetComponent<Rigidbody>();
        if (rb != null)
        {
            slot.originalRigidbody = rb;
            slot.wasKinematic = rb.isKinematic;
            slot.wasUsingGravity = rb.useGravity;
            
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        // Instantly snap to position
        interactable.transform.position = slot.attachPoint.position;
        interactable.transform.rotation = slot.attachPoint.rotation;
        
        // Subscribe to grab events
        interactable.selectEntered.AddListener(OnInteractableGrabbed);
        
        Debug.Log($"Assigned {interactable.name} to slot {slot.attachPoint.name}");
    }
    
    private void ReleaseSlot(UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable interactable, SocketSlot slot)
    {
        slot.occupyingInteractable = null;
        interactableToSlotMap.Remove(interactable);
        
        // Show preview if empty
        if (slot.previewInstance != null)
        {
            slot.previewInstance.SetActive(showPreviewWhenEmpty);
        }
        
        // Restore rigidbody state (like XRSocketInteractor does)
        if (slot.originalRigidbody != null)
        {
            slot.originalRigidbody.isKinematic = slot.wasKinematic;
            slot.originalRigidbody.useGravity = slot.wasUsingGravity;
            slot.originalRigidbody = null;
        }
        
        // Unsubscribe from events
        interactable.selectEntered.RemoveListener(OnInteractableGrabbed);
        
        Debug.Log($"Released {interactable.name} from slot {slot.attachPoint.name}");
    }
    
    private void OnInteractableGrabbed(SelectEnterEventArgs args)
    {
        UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable interactable = args.interactableObject as UnityEngine.XR.Interaction.Toolkit.Interactables.XRGrabInteractable;
        if (interactable != null && interactableToSlotMap.TryGetValue(interactable, out SocketSlot slot))
        {
            ReleaseSlot(interactable, slot);
            
            // Show preview on closest slot while holding
            ShowClosestPreview(interactable);
        }
    }
    
    public int GetOccupiedSocketCount()
    {
        int count = 0;
        foreach (var slot in slots)
        {
            if (slot.occupyingInteractable != null)
            {
                count++;
            }
        }
        return count;
    }
    
    public bool HasAvailableSocket()
    {
        foreach (var slot in slots)
        {
            if (slot.IsAvailable)
            {
                return true;
            }
        }
        return false;
    }
    
    private void OnDrawGizmosSelected()
    {
        if (socketPoints == null) return;
        
        // Draw the trigger collider bounds
        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(triggerCollider.bounds.center, triggerCollider.bounds.size);
        }
        
        // Draw socket points
        foreach (var socketTransform in socketPoints)
        {
            if (socketTransform != null)
            {
                bool isOccupied = false;
                
                if (Application.isPlaying)
                {
                    foreach (var slot in slots)
                    {
                        if (slot.attachPoint == socketTransform && slot.occupyingInteractable != null)
                        {
                            isOccupied = true;
                            break;
                        }
                    }
                }
                
                Gizmos.color = isOccupied ? Color.red : Color.green;
                Gizmos.DrawWireSphere(socketTransform.position, 0.05f);
                Gizmos.DrawLine(transform.position, socketTransform.position);
            }
        }
    }
}       