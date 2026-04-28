using UnityEngine;

public class MunchLookAt : MonoBehaviour
{
    public Transform target;

    [Header("Settings")]
    public float rotationSpeed = 5f;

    void LateUpdate()
    {
        if (target == null) return;

        // Direction to target
        Vector3 direction = target.position - transform.position;

        // Convert to rotation
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        // Smooth rotation
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            Time.deltaTime * rotationSpeed
        );
    }
}