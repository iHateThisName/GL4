using UnityEngine;

public class FireMatchBox : MonoBehaviour
{
    [SerializeField]
    private GameObject matchPrefab;

    [SerializeField]
    private Transform matchSpawnPoint;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SpawnMatch();
    }

    public void SpawnMatch()
    {
        if (matchPrefab == null)
        {
            Debug.LogError("Match Prefab is missing");
            return;
        }

        if (matchSpawnPoint == null)
        {
            Debug.LogError("Match Spawn Point is missing");
            return;
        }
        GameObject newMatch = Instantiate(matchPrefab, matchSpawnPoint.position, matchSpawnPoint.rotation);
        FireMatchController controller = newMatch.GetComponentInChildren<FireMatchController>();

        if (controller == null)
        {
            Debug.LogError("FireMatchController missing on match prefab!");
            return;
        }

        controller.MatchBox = this;
    }
}
