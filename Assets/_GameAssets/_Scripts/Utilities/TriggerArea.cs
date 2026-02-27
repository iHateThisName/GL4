using System;
using UnityEngine;

/// <summary>
/// A simple wrapper for Unity trigger events that exposes them as C# events.
/// Other scripts can subscribe to these events to react when objects enter,
/// stay inside, or exit this trigger area.
/// </summary>
public class TriggerArea : MonoBehaviour
{
    // Event fired when another collider enters this trigger.
    public event Action<Collider> OnTriggerEntered;

    // Event fired once per frame while another collider remains inside this trigger.
    public event Action<Collider> OnTriggerStayed;

    // Event fired when another collider exits this trigger.
    public event Action<Collider> OnTriggerExited;

    /// <summary>
    /// Unity callback invoked when a collider enters this trigger.
    /// Forwards the event to any subscribed listeners.
    /// </summary>
    private void OnTriggerEnter(Collider other) => OnTriggerEntered?.Invoke(other);

    /// <summary>
    /// Unity callback invoked every frame while a collider stays inside this trigger.
    /// Forwards the event to any subscribed listeners.
    /// </summary>
    private void OnTriggerStay(Collider other) => OnTriggerStayed?.Invoke(other);

    /// <summary>
    /// Unity callback invoked when a collider exits this trigger.
    /// Forwards the event to any subscribed listeners.
    /// </summary>
    private void OnTriggerExit(Collider other) => OnTriggerExited?.Invoke(other);
}