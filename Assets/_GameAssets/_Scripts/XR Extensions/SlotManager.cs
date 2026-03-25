using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class SlotManager : MonoBehaviour
{
    public XRSocketInteractor socket;
    public Transform[] attachPoints;

    private int currentSlotIndex = 0;
    private bool isProcessing = false;

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
        _ = ProcessSnapAsync(args.interactableObject);
    }

    private async Awaitable ProcessSnapAsync(IXRSelectInteractable interactable)
    {
        isProcessing = true;

        if (interactable.transform.TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        interactable.transform.SetParent(attachPoints[currentSlotIndex]);
        interactable.transform.localPosition = Vector3.zero;
        interactable.transform.localRotation = Quaternion.identity;

        socket.interactionManager.SelectExit((IXRSelectInteractor)socket, interactable);

        currentSlotIndex++;

        if (currentSlotIndex >= attachPoints.Length)
        {
            socket.socketActive = false;
            Debug.Log("Basket Full!");
        }
        else
        {
            UpdateAttachTransform();
        }

        // Small cooldown so the socket doesn't re-grab the log we just released
        await Awaitable.WaitForSecondsAsync(0.1f, destroyCancellationToken);
        isProcessing = false;
    }

    private void UpdateAttachTransform()
    {
        if (attachPoints.Length > 0 && currentSlotIndex < attachPoints.Length)
            socket.attachTransform = attachPoints[currentSlotIndex];
    }
}
