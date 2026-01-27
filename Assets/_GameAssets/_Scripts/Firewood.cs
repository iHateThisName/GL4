using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class Firewood : MonoBehaviour {
    public float FuelValue { get; private set; } = 20f;
}
