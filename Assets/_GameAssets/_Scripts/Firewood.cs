using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class Firewood : MonoBehaviour {
    [field: SerializeField] public float FuelValue { get; private set; } = 20f;
    public float RemainingFuel;
    public bool IsBurning = false;
}
