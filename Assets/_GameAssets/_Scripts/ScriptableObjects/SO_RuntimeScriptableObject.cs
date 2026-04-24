using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract base class for ScriptableObjects that hold runtime-mutable data.
/// Tracks all instances and resets them automatically before each play session.
/// </summary>
public abstract class SO_RuntimeScriptableObject : ScriptableObject
{
    // Global registry of every living instance, used for batch reset on play-mode entry
    private static readonly List<SO_RuntimeScriptableObject> Instances = new();

    /// <summary>
    /// Fired whenever runtime data on this SO changes.
    /// Listeners can update their cached copies in response.
    /// </summary>
    public event Action OnRuntimeDataChanged;

    // Register this instance when the asset is loaded or enabled
    private void OnEnable() => Instances.Add(this);

    // Unregister this instance when the asset is unloaded or disabled
    private void OnDisable() => Instances.Remove(this);

    /// <summary>
    /// Resets all runtime data on this ScriptableObject to its default state.
    /// Each concrete subclass must implement its own reset logic.
    /// </summary>
    protected abstract void OnReset();

    /// <summary>
    /// Call this from setters/methods that modify runtime data.
    /// </summary>
    protected void NotifyDataChanged()
    {
        // Invoke all registered listeners, if any
        this.OnRuntimeDataChanged?.Invoke();
    }

    /// <summary>
    /// Automatically called before the first scene loads.
    /// Clears event subscribers and resets every tracked instance.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void ResetAllInstances()
    {
        // Iterate every registered instance and restore it to a clean state
        foreach (var instance in Instances)
        {
            instance.OnRuntimeDataChanged = null; // Detach all listeners
            instance.OnReset();                   // Let subclass clear its fields
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Editor-only reset called when exiting play mode so the inspector reflects
    /// the clean asset values immediately rather than showing stale runtime state.
    /// </summary>
    public static void ResetAllForEditor()
    {
        foreach (var instance in Instances)
        {
            instance.OnRuntimeDataChanged = null;
            instance.OnReset();
        }
    }
#endif
}