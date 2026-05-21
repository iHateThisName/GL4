using UnityEngine;

public class ItemOutOfAreaScript : MonoBehaviour
{
    public Transform startPosition;

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Axe"))
        {
            other.transform.position = startPosition.position;
            other.transform.rotation = startPosition.rotation;
        }
    }
}
