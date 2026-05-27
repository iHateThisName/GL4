using UnityEngine;

public class ItemOutOfAreaScript : MonoBehaviour
{
    public Transform startPosition;

    public Rigidbody rb;

    public GameObject axe;

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Axe"))
        {
            Debug.Log("Axe should respawn");
            rb.angularVelocity = Vector3.zero;
            rb.linearVelocity = Vector3.zero;
            axe.transform.position = startPosition.position;
            axe.transform.rotation = startPosition.rotation;
            Debug.Log("Axe should move");
        }
    }
}
