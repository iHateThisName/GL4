using UnityEngine;

public class FireMatchBox : MonoBehaviour
{
    //A refrence to the fire match prefab which will be spawned by the match box
    [SerializeField]
    private GameObject matchPrefab;

    //The spawn point which the match will spawned in
    [SerializeField]
    private Transform matchSpawnPoint;

    //A bool that is supposed to stop the match from duplicating, doesn't work
    public bool MatchSpawned = false;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Spawn a match at game start
        SpawnMatch();
    }

    public void SpawnMatch()
    {
        //If there is already a match spawned, a new match should not be spawned
        if (!MatchSpawned)
        {
            //Check for if the refrence to the match prefab is missing
            if (matchPrefab == null)
            {
                Debug.LogError("Match Prefab is missing");
                return;
            }

            //Check for if the refrence to the match spawn point is missing
            if (matchSpawnPoint == null)
            {
                Debug.LogError("Match Spawn Point is missing");
                return;
            }

            //Spawn in a match at the match spawn point
            GameObject newMatch = Instantiate(matchPrefab, matchSpawnPoint.position, matchSpawnPoint.rotation);
            //Gets a refrence to the match's FireMatchController
            FireMatchController controller = newMatch.GetComponentInChildren<FireMatchController>();

            //Check for if the match doesn't have a FireMatchController
            if (controller == null)
            {
                Debug.LogError("FireMatchController missing on match prefab!");
                return;
            }

            //Sets a refrence in the match for this matchbox
            controller.MatchBox = this;
            //This should make sure that the match doesn't duplicate 
            MatchSpawned = true;
        }
    }
}
