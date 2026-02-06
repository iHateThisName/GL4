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

    [Tooltip("How far apart the two pieces should spawn relative to the axe blade.")]
    [SerializeField] private float splitWidth = 0.04f;

    private XRGrabInteractable grabInteractable;

    #region Unity Lifecycle
    // Caches the interactable component
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
                    // Pass the axe collider to determine split orientation
                    this.SpawnPieces(other.transform);
                }
            }
        }
    }
    #endregion

    #region Logic Checks
    // Measures speed of the strike via Rigidbody linearVelocity
    private bool IsMovingFastEnough(Collider axeCollider)
    {
        Rigidbody axeRigidbody = axeCollider.attachedRigidbody;

        if (axeRigidbody != null)
        {
            float currentSpeed = axeRigidbody.linearVelocity.magnitude;
            return currentSpeed >= this.minChopVelocity;
        }

        return false;
    }

    // Verifies the log is currently in a socket
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
    // Spawns pieces aligned with the axe's rotation
    private void SpawnPieces(Transform axeTransform)
    {
        // 1. Calculate the 'split direction' (perpendicular to the axe blade)
        // Usually, the 'right' vector of the axe blade represents the sideways push
        Vector3 splitDirection = axeTransform.right;

        // 2. Calculate positions: Move piece 1 left and piece 2 right relative to the axe
        Vector3 spawnPos1 = this.transform.position + (splitDirection * (this.splitWidth * 0.5f));
        Vector3 spawnPos2 = this.transform.position - (splitDirection * (this.splitWidth * 0.5f));

        // 3. Spawn pieces rotated to match the axe's orientation for a 'clean' look
        // We keep the log's original rotation but adjust it to the axe's Y rotation
        Quaternion splitRotation = Quaternion.Euler(0, axeTransform.eulerAngles.y, 0);

        Instantiate(this.firewood1, spawnPos1, splitRotation);
        Instantiate(this.firewood2, spawnPos2, splitRotation);

        // 4. Clean up original log
        Destroy(this.gameObject);
    }
    #endregion
}