using UnityEngine;

public class ItemOutOfAreaScript : MonoBehaviour
{

    public GameObject areaRange;

    private Vector3 startPosition;


    private void Start()
    {
        startPosition = transform.position;
    }

    private void OnTriggerExit(Collider areaRange)
    {
        transform.position = startPosition;
    }
}
