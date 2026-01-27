using UnityEngine;

public class Choppable_Wood : MonoBehaviour
{
    [SerializeField] GameObject prefab1;
    [SerializeField] GameObject prefab2;
    [SerializeField] AreaTrigger areaTrigger;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Finish"))
        {
            // Check if inside the trigger zone
            if (areaTrigger != null && areaTrigger.IsInside())
            {
                // Spawn two prefabs
                Instantiate(prefab1, transform.position + new Vector3(-0.02f, 0f, 0f), Quaternion.identity);
                Instantiate(prefab2, transform.position + new Vector3(0.02f, 0f, 0f), Quaternion.identity);

                areaTrigger.SetInside(false);
                // Destroy big wood
                Destroy(gameObject);
            }
        }
    }
}
