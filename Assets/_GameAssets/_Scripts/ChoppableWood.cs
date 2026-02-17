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

    [Tooltip("How far apart the pieces spawn relative to the axe blade.")]
    [SerializeField] private float splitWidth = 0.04f;

    private XRGrabInteractable grabInteractable;

    #region Unity Lifecycle
    private void Awake()
    {
        this.grabInteractable = this.GetComponent<XRGrabInteractable>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(this.tagInQuestion))
        {
            if (this.IsMovingFastEnough(other))
            {
                if (this.IsSnappedToSocket())
                {
                    this.SpawnPieces(other.transform);
                }
            }
        }
    }
    #endregion

    #region Logic Checks
    private bool IsMovingFastEnough(Collider axeCollider)
    {
        Rigidbody axeRigidbody = axeCollider.attachedRigidbody;
        if (axeRigidbody != null)
        {
            return axeRigidbody.linearVelocity.magnitude >= this.minChopVelocity;
        }
        return false;
    }

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
    private void SpawnPieces(Transform axeTransform)
    {
        // 1. Position Logic: Using the specific axis that matched your visual gizmo
        Vector3 splitDirection = axeTransform.forward;

        Vector3 spawnPos1 = this.transform.position + (splitDirection * (this.splitWidth * 0.5f));
        Vector3 spawnPos2 = this.transform.position - (splitDirection * (this.splitWidth * 0.5f));

        // 2. Rotation Logic: Correcting the 90-degree offset to align faces with the blade
        float correctedY = axeTransform.eulerAngles.y + 90f;
        Quaternion splitRotation = Quaternion.Euler(0, correctedY, 0);

        Instantiate(this.firewood1, spawnPos1, splitRotation);
        Instantiate(this.firewood2, spawnPos2, splitRotation);

        Destroy(this.gameObject);
    }
    #endregion
}