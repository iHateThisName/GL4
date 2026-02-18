using System.Collections;
using UnityEngine;

public class TheMunch : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator monsterAnimator;
    [SerializeField] private string munchTriggerName = "Munch";
    [SerializeField] private string returnTriggerName = "Return";

    [Tooltip("How long the hand stays at the 'End' position before returning")]
    [SerializeField] private float holdDuration = 10.0f;

    [Header("Rejection Settings")]
    [Range(1, 20)]
    [Tooltip("The force applied when 'flicking' away non-food items")]
    [SerializeField] private float throwForce = 5f;

    public Vector3 ThrowDirection = new Vector3(0, 1, 1);

    private bool isMunching = false;

    private void OnTriggerEnter(Collider other)
    {
        if (this.isMunching) return;

        Food foodItem = other.GetComponentInParent<Food>();

        if (foodItem != null)
        {
            StartCoroutine(this.MunchSequence(foodItem.gameObject));
        }
        else
        {
            this.RejectItem(other.gameObject);
        }
    }

    private IEnumerator MunchSequence(GameObject food)
    {
        this.isMunching = true;

        // 1. Play the 'Grab' animation
        if (this.monsterAnimator != null)
        {
            this.monsterAnimator.SetTrigger(this.munchTriggerName);
        }

        // 2. Hide/Destroy the food once it's 'grabbed'
        // You might want to parent the food to the hand here before destroying
        Destroy(food, 0.2f);

        // 3. FREEZE: Wait at the end of the grab animation
        yield return new WaitForSeconds(this.holdDuration);

        // 4. Play the 'Return' animation
        if (this.monsterAnimator != null)
        {
            this.monsterAnimator.SetTrigger(this.returnTriggerName);
        }

        // Wait for return animation to finish (optional) before allowing next munch
        yield return new WaitForSeconds(1.0f);
        this.isMunching = false;
    }

    private void RejectItem(GameObject item)
    {
        Rigidbody rb = item.GetComponentInParent<Rigidbody>();

        if (rb != null)
        {
            Vector3 worldThrowDir = this.transform.TransformDirection(this.ThrowDirection);
            rb.AddForce(worldThrowDir * this.throwForce, ForceMode.Impulse);
        }
    }

    // Getters / Setters
    public float GetHoldDuration() => this.holdDuration;
    public void SetHoldDuration(float duration) => this.holdDuration = duration;
}