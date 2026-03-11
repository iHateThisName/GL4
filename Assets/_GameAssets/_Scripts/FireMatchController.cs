using System;
using System.Collections;
using UnityEngine;

public class FireMatchController : MonoBehaviour 
{

    [SerializeField] private GameObject rootObject;

    //A refrence to the fire VFX on the fire match
    [SerializeField] private GameObject fireVFX;

    //The time it takes for the match to despawn
    [SerializeField]
    private float despawnTimer;


    //A refrence to the match box
    public FireMatchBox MatchBox;

    public static event Action OnMatchDespawn;

    //A method for despawning the match once the player releases the match
    public void StartDespawnTimer()
    {
        //Calling the corroutine that despawns the match after a certain time
        StartCoroutine(DespawnMatch());
    }

    //The coroutine that despawns the match after a set time
    IEnumerator DespawnMatch()
    {
        Debug.Log("Start match despawn");
        //A timer
        yield return new WaitForSeconds(despawnTimer);
        //Destroying the match after the time runs out
        OnMatchDespawn?.Invoke();
        Destroy(this.transform.parent.gameObject);
    }
}
