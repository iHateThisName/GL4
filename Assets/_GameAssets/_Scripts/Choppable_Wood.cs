using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Choppable_Wood : MonoBehaviour
{
    [SerializeField] GameObject prefab1;
    [SerializeField] GameObject prefab2;

    private XRGrabInteractable grabInteractable;

    private void Awake()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
    }

    // Use Trigger instead of Collision for instant detection while held
    private void OnTriggerEnter(Collider other)
    {
        // 1. Check tag
        if (other.CompareTag("Finish"))
        {
            // 2. Ensure it's in the socket
            if (IsSnappedToSocket())
            {
                SpawnPieces();
            }
        }
    }

    private bool IsSnappedToSocket()
    {
        // Check if the wood is currently being held by a socket
        if (grabInteractable != null && grabInteractable.isSelected)
        {
            var interactor = grabInteractable.firstInteractorSelecting;
            return interactor is UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor;
        }
        return false;
    }

    private void SpawnPieces()
    {
        Instantiate(prefab1, transform.position + new Vector3(-0.02f, 0f, 0f), transform.rotation);
        Instantiate(prefab2, transform.position + new Vector3(0.02f, 0f, 0f), transform.rotation);

        Destroy(gameObject);
    }
}