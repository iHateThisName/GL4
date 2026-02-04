using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ChoppableWood : MonoBehaviour
{
    [Header("Chopping Configuration")]
    [SerializeField] private GameObject firewood1;
    [SerializeField] private GameObject firewood2;
    [SerializeField] private string tagInQuestion;

    private XRGrabInteractable grabInteractable;

    #region Unity Lifecycle
    // Automatically gets the grabInteractable component reference
    private void Awake()
    {
        this.grabInteractable = GetComponent<XRGrabInteractable>();
    }

    // Listens for collisions with the chopping tool
    private void OnTriggerEnter(Collider other)
    {
        // 1. Look for tagInQuestion
        if (other.CompareTag(this.tagInQuestion))
        {
            // 2. Ensure it's in the socket
            if (this.IsSnappedToSocket())
            {
                this.SpawnPieces();
            }
        }
    }
    #endregion

    #region Logic Checks
    // Checks if the wood is currently being held by a socket interactor
    private bool IsSnappedToSocket()
    {
        if (this.grabInteractable != null && this.grabInteractable.isSelected)
        {
            return this.grabInteractable.firstInteractorSelecting is XRSocketInteractor;
        }
        return false;
    }
    #endregion

    #region Actions
    // Spawns the chopped firewood prefabs and removes the original log
    private void SpawnPieces()
    {
        Instantiate(this.firewood1, this.transform.position + new Vector3(-0.02f, 0f, 0f), this.transform.rotation);
        Instantiate(this.firewood2, this.transform.position + new Vector3(0.02f, 0f, 0f), this.transform.rotation);

        Destroy(this.gameObject);
    }
    #endregion
}