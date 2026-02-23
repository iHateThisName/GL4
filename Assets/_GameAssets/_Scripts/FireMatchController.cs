using System.Collections;
using UnityEngine;

public class FireMatchController : MonoBehaviour {
    [SerializeField] private GameObject rootObject;
    public GameObject RootObject => this.rootObject;

    [SerializeField] private GameObject fireVFX;

    [SerializeField]
    private float despawnTimer;

    [SerializeField]
    private GameObject flame;

    [SerializeField]
    private Rigidbody rb;

    public FireMatchBox MatchBox;

    public bool IsIgnited { get; private set; }

    private void Start() {
        this.IsIgnited = this.fireVFX.gameObject.activeSelf;
    }


    public void Ignite() {
        if (this.IsIgnited) return; // Already ignited, do nothing

        this.IsIgnited = true;
        fireVFX.SetActive(true);
    }

    public void InstatiateSelf() {
        GameObject newFireMatch = Instantiate(this.rootObject, this.transform.position, this.transform.rotation);
    }

    public void EnableModel()
    {
        rb.isKinematic = false;
        flame.SetActive(true);
    }

    public void StartDespawnTimer()
    {
        MatchBox.SpawnMatch();
        StartCoroutine(DespawnMatch());
    }

    IEnumerator DespawnMatch()
    {
        Debug.Log("Start match despawn");
        yield return new WaitForSeconds(despawnTimer);
        Destroy(this.transform.root.gameObject);
    }
}
