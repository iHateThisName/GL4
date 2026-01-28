using System;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class Food : MonoBehaviour
{
    [SerializeField] private float foodValue;
    [SerializeField] private XRGrabInteractable grabInteractable;

    private void OnEnable()
    {
        if (this.grabInteractable == null) return;
        this.grabInteractable.selectEntered.AddListener(HandleGrabbed);
    }

    private void OnDisable()
    {
        if (this.grabInteractable == null) return;
        this.grabInteractable.selectEntered.RemoveListener(HandleGrabbed);
    }

    private void HandleGrabbed(SelectEnterEventArgs args)
    {
        Debug.Log("Grabbed food");
    }

    public float GetFoodValue() => this.foodValue;
}
