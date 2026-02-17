using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class Firewood : MonoBehaviour {
    [field: SerializeField] public float FuelValue { get; private set; } = 20f; // Total fuel value of this firewood piece.
    public float RemainingFuel; // Remaining fuel left in this firewood piece will be initialized when added to fireplace.
    public bool IsBurning = false; // Indicates if this firewood is currently burning and cant be added again to the fireplace.
}
