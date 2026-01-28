using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SlotManager : MonoBehaviour
{
    public XRSocketInteractor socket;
    public Transform[] attachPoints;

    private int currentSlotIndex = 0;
    private bool isProcessing = false; // Prevents the "Double Snap" bug

    void OnEnable()
    {
        socket.selectEntered.AddListener(OnObjectSnapped);
        UpdateAttachTransform();
    }

    void OnDisable()
    {
        socket.selectEntered.RemoveListener(OnObjectSnapped);
    }

    private void OnObjectSnapped(SelectEnterEventArgs args)
    {
        if (isProcessing) return;
        StartCoroutine(ProcessSnap(args.interactableObject));
    }

    private IEnumerator ProcessSnap(IXRSelectInteractable interactable)
    {
        isProcessing = true;

        // 1. Physics Safety: Stop the wood from moving/bouncing
        if (interactable.transform.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // 2. Parent to the current slot
        interactable.transform.SetParent(attachPoints[currentSlotIndex]);
        interactable.transform.localPosition = Vector3.zero;
        interactable.transform.localRotation = Quaternion.identity;

        // 3. Force the socket to release the wood
        socket.interactionManager.SelectExit((IXRSelectInteractor)socket, interactable);

        // 4. Advance to the next slot
        currentSlotIndex++;

        if (currentSlotIndex >= attachPoints.Length)
        {
            socket.socketActive = false; // Basket is full
            Debug.Log("Basket Full!");
        }
        else
        {
            UpdateAttachTransform();
        }

        // 5. Small cooldown so the socket doesn't re-grab the log we just released
        yield return new WaitForSeconds(0.1f);
        isProcessing = false;
    }

    private void UpdateAttachTransform()
    {
        if (attachPoints.Length > 0 && currentSlotIndex < attachPoints.Length)
        {
            socket.attachTransform = attachPoints[currentSlotIndex];
        }
    }
}