using UnityEngine;

/// <summary>
/// Holds runtime-only references to cross-system objects (e.g., the Player).
/// Objects are registered on Awake/Start (e.g., by GameManager).
/// Automatically reset before each play session via SO_RuntimeScriptableObject.
/// </summary>
[CreateAssetMenu(menuName = "Runtime/References")]
public class SO_RuntimeReferences : SO_RuntimeScriptableObject
{
    // Cached reference to the player's Transform, set at runtime by the GameManager
    [System.NonSerialized] private Transform player;

    /// <summary>
    /// Gets or sets the runtime reference to the player's Transform.
    /// </summary>
    public Transform Player { get => this.player; set => this.player = value; }

    /// <summary>
    /// Clears all runtime references so the SO starts clean each play session.
    /// </summary>
    protected override void OnReset()
    {
        // Release the player reference so it doesn't survive between sessions
        this.player = null;
    }
}
