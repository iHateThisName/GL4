using UnityEngine;

public class FireMatchController : MonoBehaviour {
    [SerializeField] private GameObject rootObject;
    public GameObject RootObject => this.rootObject;

    [SerializeField] private GameObject fireVFX;

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

}
