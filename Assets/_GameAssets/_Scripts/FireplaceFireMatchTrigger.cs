using UnityEngine;

public class FireplaceFireMatchTrigger : MonoBehaviour {

    [SerializeField] private FireplaceController fireplaceController;


    private void OnTriggerEnter(Collider other) {

        if (!other.CompareTag("FireMatch")) return;

        if (other.CompareTag("FireMatch")) {
            Debug.Log("Fireplace fuel trigger detected collision with: " + other.gameObject.name);
            FireMatchController fireMatchController = other.transform.root.GetComponentInChildren<FireMatchController>();
            IgniteFireplace(fireMatchController);
        }
    }

    private void IgniteFireplace(FireMatchController fireMatchController) {
        if (!this.fireplaceController.HasFuel) {
            Debug.Log("Cannot ignite fireplace: No fuel present.");
            return;
        }
        if (this.fireplaceController.IsLit) {
            Debug.Log("Fireplace is already lit.");
            return;
        }

        this.fireplaceController.Ignite(fireMatchController);

    }
}
