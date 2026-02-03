using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class Choppable_Wood : MonoBehaviour
{
    [SerializeField] GameObject firewood1;
    [SerializeField] GameObject firewood2;
    [SerializeField] string tagInQuestion;

    private XRGrabInteractable grabInteractable;

    private void Awake()
    {
        //Automatically gets the grabInteractable component
        this.grabInteractable = GetComponent<XRGrabInteractable>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // 1. Look for tagInQuestion
        if (other.CompareTag(tagInQuestion))
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
        if (this.grabInteractable != null && this.grabInteractable.isSelected)
        {
            var interactor = this.grabInteractable.firstInteractorSelecting;
            return interactor is UnityEngine.XR.Interaction.Toolkit.Interactors.XRSocketInteractor;
        }
        return false;
    }

    private void SpawnPieces()
    {
        //Spawns the two "chopped" wood prefabs
        Instantiate(this.firewood1, transform.position + new Vector3(-0.02f, 0f, 0f), transform.rotation);
        Instantiate(this.firewood2, transform.position + new Vector3(0.02f, 0f, 0f), transform.rotation);

        Destroy(this.gameObject);
    }
}