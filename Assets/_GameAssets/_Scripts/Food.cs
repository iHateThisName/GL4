using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Represents a food item that can be grabbed in XR.
/// Provides a food value and listens for grab events.
/// </summary>
[RequireComponent(typeof(XRGrabInteractable))] // Ensures the object always has an XRGrabInteractable component
public class Food : MonoBehaviour
{
    [SerializeField] private float foodValue = 10f; // Amount of "nutrition" or value this food provides to the player

    [SerializeField] private XRGrabInteractable grabInteractable; 
    // Reference to the XRGrabInteractable component.
    // Handles grab interactions such as when the player picks up the food.

    /// <summary>
    /// Unity callback invoked when the object becomes enabled.
    /// Subscribes to grab events so the script can react when the food is picked up.
    /// </summary>
    private void OnEnable()
    {
        if (this.grabInteractable == null) return; // Safety check in case the reference is missing
        this.grabInteractable.selectEntered.AddListener(HandleGrabbed); // Register grab event listener
    }

    /// <summary>
    /// Unity callback invoked when the object becomes disabled.
    /// Unsubscribes from grab events to prevent memory leaks or duplicate event calls.
    /// </summary>
    private void OnDisable()
    {
        if (this.grabInteractable == null) return; // Safety check
        this.grabInteractable.selectEntered.RemoveListener(HandleGrabbed); // Remove grab event listener
    }

    /// <summary>
    /// Called when the food item is grabbed by an XR interactor (e.g., VR hand).
    /// Useful for triggering sound effects, animations, or gameplay logic.
    /// </summary>
    /// <param name="args">Event data containing information about the interactor.</param>
    private void HandleGrabbed(SelectEnterEventArgs args)
    {
        Debug.Log("Grabbed food"); // Simple debug output for testing interaction
    }

    /// <summary>
    /// Returns the food value associated with this item.
    /// Used by hunger or stamina systems to determine how much benefit the player receives.
    /// </summary>
    public float GetFoodValue() => this.foodValue;
}
