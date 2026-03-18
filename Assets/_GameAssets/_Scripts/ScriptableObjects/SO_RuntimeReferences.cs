using System.Linq;
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

    // Cached reference to the Radio, set at runtime
    [System.NonSerialized] private Radio radio;

    [System.NonSerialized] private readonly System.Collections.Generic.List<WindowController>  windows;

    /// <summary>
    /// Gets or sets the runtime reference to the player's Transform.
    /// </summary>
    public Transform Player { get => this.player; set => this.player = value; }

    /// <summary>
    /// Gets or sets the runtime reference to the Radio.
    /// </summary>
    public Radio Radio { get => this.radio; set => this.radio = value; }
    
    /// <summary>
    /// Gets the runtime reference to the windows.
    /// </summary>
    public WindowController[] Windows => this.windows.ToArray();
    
    /// <summary>
    /// Adds a runtime reference for a window.
    /// </summary>
    public void RegisterWindow(WindowController window) => this.windows.Add(window);
    
    /// <summary>
    /// Removes a runtime reference for a window.
    /// </summary>
    public void DeregisterWindow(WindowController window) => this.windows.Remove(window);

    /// <summary>
    /// Gets only the closed windows.
    /// </summary>
    public WindowController[] ClosedWindows =>
        Windows?.Where(w => w != null && w.GetCurrentWindowState() == VRLever.EnumLeverState.Closed).ToArray()
        ?? System.Array.Empty<WindowController>();

    /// <summary>
    /// Clears all runtime references so the SO starts clean each play session.
    /// </summary>
    protected override void OnReset()
    {
        // Release references so they don't survive between sessions
        this.player = null;
        this.radio = null;
    }
}
