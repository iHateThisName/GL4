using System.Collections;
using UnityEngine;

public class FireMatchController : MonoBehaviour {

    [SerializeField] private GameObject rootObject;
    public GameObject RootObject => this.rootObject;

    //A refrence to the fire VFX on the fire match
    [SerializeField] private GameObject fireVFX;

    //The time it takes for the match to despawn
    [SerializeField]
    private float despawnTimer;

    //A refrence to the match's rigidbody
    [SerializeField]
    private Rigidbody rb;

    //A refrence to the match box
    public FireMatchBox MatchBox;

    //A method for ligthing the match once it is grabbed
    public void LightMatch()
    {
        //Make it so that the box knows that the match has been taken
        MatchBox.MatchSpawned = false;
        //Make the match affected by physics
        rb.isKinematic = true;
        //Enabling the VFX for the fire
        fireVFX.SetActive(true);
    }

    //A method for despawning the match once the player releases the match
    public void StartDespawnTimer()
    {
        //Calling the corroutine that despawns the match after a certain time
        StartCoroutine(DespawnMatch());
        //Check that is supposed to stop the match from duplicating
        if (MatchBox.MatchSpawned)
        {
            return;
        }
        //Making the match box spawning a new match
        MatchBox.SpawnMatch();
    }

    //The coroutine that despawns the match after a set time
    IEnumerator DespawnMatch()
    {
        Debug.Log("Start match despawn");
        //A timer
        yield return new WaitForSeconds(despawnTimer);
        //Destroying the match after the time runs out
        Destroy(this.transform.root.gameObject);
    }
}
