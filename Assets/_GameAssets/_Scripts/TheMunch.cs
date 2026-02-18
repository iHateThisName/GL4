using UnityEngine;

public class TheMunch : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator monsterAnimator;
    [SerializeField] private string munchTriggerName = "Munch";

    [Header("Rejection Settings")]
    [Range(1, 20)]
    [Tooltip("The force applied when 'flicking' away non-food items")]
    [SerializeField] private float throwForce = 5f;

    public Vector3 ThrowDirection = new Vector3(0, 1, 1);

    private void OnTriggerEnter(Collider other)
    {
        // Using GetComponentInParent since the collider is a child of the food logic
        Food foodItem = other.GetComponentInParent<Food>();

        if (foodItem != null)
        {
            this.AcceptFood(foodItem.gameObject);
        }
        else
        {
            this.RejectItem(other.gameObject);
        }
    }

    private void AcceptFood(GameObject food)
    {
        if (this.monsterAnimator != null)
        {
            this.monsterAnimator.SetTrigger(this.munchTriggerName);
        }

        // Small delay to allow the animation to play before the object vanishes
        Destroy(food, 0.2f);
    }

    private void RejectItem(GameObject item)
    {
        // Look for Rigidbody on the parent
        Rigidbody rb = item.GetComponentInParent<Rigidbody>();

        if (rb != null)
        {
            // Transform the local direction to world space relative to the hand
            Vector3 worldThrowDir = this.transform.TransformDirection(this.ThrowDirection);

            rb.AddForce(worldThrowDir * this.throwForce, ForceMode.Impulse);
        }
    }

    // Getters / Setters per convention
    public float GetThrowForce() => this.throwForce;
    public void SetThrowForce(float newForce) => this.throwForce = newForce;
}