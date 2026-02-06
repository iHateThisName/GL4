using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ChoppableWood : MonoBehaviour
{
    [Header("Chopping Configuration")]
    [SerializeField] private GameObject firewood1;
    [SerializeField] private GameObject firewood2;
    [SerializeField] private string tagInQuestion;

    [Tooltip("The minimum speed (m/s) the axe must be moving to split the wood.")]
    [SerializeField] private float minChopVelocity = 2.0f;

    private XRGrabInteractable grabInteractable;

    #region Unity Lifecycle
    // Gets the grabInteractable component reference
    private void Awake()
    {
        this.grabInteractable = this.GetComponent<XRGrabInteractable>();
    }

    // Listens for tool collision and validates speed/socket state
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(this.tagInQuestion))
        {
            if (this.IsMovingFastEnough(other))
            {
                if (this.IsSnappedToSocket())
                {
                    this.SpawnPieces();
                }
            }
        }
    }
    #endregion

    #region Logic Checks
    // Checks the speed of the axe using its Rigidbody
    private bool IsMovingFastEnough(Collider axeCollider)
    {
        // We look for the Rigidbody on the axe or its parent
        Rigidbody axeRigidbody = axeCollider.attachedRigidbody;

        if (axeRigidbody != null)
        {
            // linearVelocity.magnitude provides the speed in meters per second.
            // Note: This requires the Axe's Grab Interactable to be set to 'Velocity Tracking' movement type.
            float currentSpeed = axeRigidbody.linearVelocity.magnitude;

            // Optional: Debug.Log("Axe Speed: " + currentSpeed);
            return currentSpeed >= this.minChopVelocity;
        }

        return false;
    }

    // Verifies the wood is currently secured in a socket
    private bool IsSnappedToSocket()
    {
        if (this.grabInteractable != null && this.grabInteractable.isSelected)
        {
            // Check if the current interactor is a socket
            return this.grabInteractable.firstInteractorSelecting is XRSocketInteractor;
        }
        return false;
    }
    #endregion

    #region Actions
    // Spawns pieces and removes the original log
    private void SpawnPieces()
    {
        Instantiate(this.firewood1, this.transform.position + new Vector3(-0.02f, 0f, 0f), this.transform.rotation);
        Instantiate(this.firewood2, this.transform.position + new Vector3(0.02f, 0f, 0f), this.transform.rotation);

        Destroy(this.gameObject);
    }
    #endregion
}