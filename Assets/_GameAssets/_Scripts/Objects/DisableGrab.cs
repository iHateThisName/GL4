using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class DisableGrab : MonoBehaviour
{
    [SerializeField] private XRGrabInteractable grabInteractable;
    
    private void OnEnable()
    {
        grabInteractable.enabled = false;
    }
    
    private void OnDisable()
    {
        grabInteractable.enabled = true;
    }
}