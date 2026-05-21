using UnityEngine;

public class ItemOutOfAreaScript : MonoBehaviour
{
    public Transform startPosition;

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Axe"))
        {
            Debug.Log("Axe should respawn");
            other.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            other.GetComponentInParent<Rigidbody>().isKinematic = true;
            other.transform.position = startPosition.position;
            other.transform.rotation = startPosition.rotation;
            other.GetComponentInParent<Rigidbody>().isKinematic = false;
            Debug.Log("Axe should move");
        }
    }
}
