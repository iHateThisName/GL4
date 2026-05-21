using UnityEngine;

public class ItemOutOfAreaScript : MonoBehaviour
{
    public Transform startPosition;

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Axe"))
        {
            Debug.Log("Axe should respawn");
            other.transform.position = startPosition.position;
            other.transform.rotation = startPosition.rotation;
            Debug.Log("Axe should move");
        }
    }
}
