using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class TheMunch : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator monsterAnimator;
    [SerializeField] private string munchTriggerName = "Munch";
    [SerializeField] private string returnTriggerName = "Return";
    [SerializeField] private string rejectTriggerName = "Reject";

    [Tooltip("How long the hand stays at the 'End' position before returning")]
    [SerializeField] private float holdDuration = 2.0f;

    [Header("Interaction Settings")]
    [Range(0, 10)]
    [SerializeField] private float maxAcceptableVelocity = 2.0f;

    [Header("Rejection Settings")]
    [Range(1, 20)]
    [SerializeField] private float throwForce = 5f;

    public Vector3 ThrowDirection = new Vector3(0, 1, 1);

    private bool isMunching = false;

    private void OnTriggerEnter(Collider other)
    {
        if (this.isMunching) return;

        Rigidbody parentRb = other.attachedRigidbody;
        if (parentRb == null) return;

        Food foodItem = parentRb.GetComponent<Food>();

        if (foodItem != null)
        {
            if (this.IsMovingTooFast(parentRb))
            {
                this.RejectItem(parentRb);
            }
            else
            {
                StartCoroutine(this.MunchSequence(foodItem.gameObject));
            }
        }
        else
        {
            this.RejectItem(parentRb);
        }
    }

    private bool IsMovingTooFast(Rigidbody rb)
    {
        return rb.linearVelocity.magnitude > this.maxAcceptableVelocity;
    }

    private IEnumerator MunchSequence(GameObject food)
    {
        this.isMunching = true;

        // Force release if the player is holding the food they are trying to feed
        this.ForceRelease(food.GetComponent<XRGrabInteractable>());

        if (this.monsterAnimator != null)
        {
            this.monsterAnimator.SetTrigger(this.munchTriggerName);
        }

        Destroy(food, 0.2f);

        yield return new WaitForSeconds(this.holdDuration);

        if (this.monsterAnimator != null)
        {
            this.monsterAnimator.SetTrigger(this.returnTriggerName);
        }

        yield return new WaitForSeconds(1.0f);
        this.isMunching = false;
    }

    private void RejectItem(Rigidbody rb)
    {
        if (this.monsterAnimator != null)
        {
            this.monsterAnimator.SetTrigger(this.rejectTriggerName);
        }
        // 1. Force the player to let go so physics can take over
        XRGrabInteractable grabInteractable = rb.GetComponent<XRGrabInteractable>();
        this.ForceRelease(grabInteractable);

        // 2. Apply the slap force
        Vector3 worldThrowDir = this.transform.TransformDirection(this.ThrowDirection);
        rb.AddForce(worldThrowDir * this.throwForce, ForceMode.Impulse);

        Debug.Log($"Monster slapped {rb.name} out of player's hand!");
    }

    private void ForceRelease(XRGrabInteractable interactable)
    {
        if (interactable != null && interactable.isSelected)
        {
            // Get the interaction manager and tell it to cancel the current selection
            IXRInteractable iInteractable = interactable;
            interactable.interactionManager.SelectExit(interactable.firstInteractorSelecting, interactable);
        }
    }

    #region Getters/Setters
    public float GetMaxAcceptableVelocity() => this.maxAcceptableVelocity;
    public void SetMaxAcceptableVelocity(float velocity) => this.maxAcceptableVelocity = velocity;
    #endregion
}