using System;
using System.Collections;
using UnityEngine;

public class FireMatchController : MonoBehaviour 
{

    [SerializeField] private GameObject rootObject;

    //[SerializeField] private GameObject fireVFX;

    [SerializeField]
    private float despawnTimer;

    public FireMatchBox MatchBox;

    public void StartDespawnTimer()
    {
        StartCoroutine(DespawnMatch());
    }

    //The coroutine that despawns the match after a set time
    IEnumerator DespawnMatch()
    {
        Debug.Log("Start match despawn");
        yield return new WaitForSeconds(despawnTimer);
        Destroy(this.transform.parent.gameObject);
    }
}
