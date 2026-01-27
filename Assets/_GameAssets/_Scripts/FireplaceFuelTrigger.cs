using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class FireplaceFuelTrigger : MonoBehaviour {

    [SerializeField] private FireplaceController fireplaceController;

    private void OnTriggerStay(Collider other) {

        // Check if the colliding object is firewood
        Firewood wood = other.GetComponent<Firewood>();

        if (wood != null) return; // Only proceed if it's firewood

        Debug.Log("Firewood detected in fireplace fuel trigger.");

        // Check if the firewood is being held by the player
        XRGrabInteractable grab = other.GetComponent<XRGrabInteractable>();
        if (grab == null) return;

        Debug.Log("Firewood is grabbable.");

        // Only consume when player lets go of the firewood
        if (!grab.isSelected) {
            Debug.Log("Firewood released, adding fuel to fireplace.");
            AddFuelToFireplace(wood);
        }
    }

    private void AddFuelToFireplace(Firewood wood) {
        // Add some randomness to the fuel value to make it feel more organic
        float randomizedFuelValue = Random.Range(wood.FuelValue * 0.8f, wood.FuelValue * 1.2f);
        this.fireplaceController.AddFuel(randomizedFuelValue);

        // Destroy the firewood after adding its fuel value
        Destroy(wood.gameObject);
    }
}
