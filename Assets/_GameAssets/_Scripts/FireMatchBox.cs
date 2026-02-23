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
        GameObject newMatch = Instantiate(matchPrefab);
        newMatch.transform.position = matchSpawnPoint.position;
        newMatch.transform.rotation = matchSpawnPoint.rotation;
        newMatch.GetComponent<FireMatchController>().MatchBox = this;
    }
}
