using UnityEngine;

public class BreakableGlassController : MonoBehaviour {
    [SerializeField] private float breakForce = 10f;
    [SerializeField] private Transform shatteredGlass;
    [SerializeField] private Transform intactGlass;

    private void OnCollisionEnter(Collision collision) {
        if (collision.relativeVelocity.magnitude >= breakForce) {
            BreakGlass();
        }
    }

    private void BreakGlass() {
        this.intactGlass.gameObject.SetActive(false);
        this.shatteredGlass.parent = null;
        this.shatteredGlass.gameObject.SetActive(true);
        Destroy(shatteredGlass, 60f);
        Destroy(this.transform.root, 60f);

        foreach (Collider collider in GetComponents<Collider>()) {
            collider.isTrigger = true;
            collider.enabled = false;
        }
    }
}
