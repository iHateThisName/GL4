using UnityEngine;

public class ItemOutOfAreaScript : MonoBehaviour
{
    public Transform startPosition;

    private void OnTriggerExit(Collider areaRange)
    {
        if(areaRange.CompareTag("Axe"))
        {
            areaRange.transform.position = startPosition.position;
            areaRange.transform.rotation = startPosition.rotation;
        }
    }
}
