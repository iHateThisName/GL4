using UnityEngine;

public class MunchFollow : MonoBehaviour
{
    public Transform target;

    [Header("Settings")]
    public float rotationSpeed = 5f; // smoothness

    private void Start() {
        this.target = GameObject.FindWithTag("Player").transform.root.transform;
    }
    void FixedUpdate()
    {
        if (target == null) return;

        // Direction to target
        Vector3 direction = target.position - transform.position;

        if (direction.sqrMagnitude < 0.001f) return;

        // Create rotation where Z axis looks at target
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Smooth rotation
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.fixedDeltaTime
        );
    }
}