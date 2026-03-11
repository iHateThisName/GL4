using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

/// <summary>
/// Initializes custom systems by injecting them into Unity's PlayerLoopSystem.
/// Runs automatically when assemblies are loaded, before any scene loads.
/// </summary>
/// <remarks>
/// PlayerLoopSystem integration allows custom systems to run without MonoBehaviour overhead.
/// This is more performant than using Update() methods, especially for systems that need
/// to run every frame like TimerManager.
/// </remarks>
internal static class SystemsBootstrap
{
    private static PlayerLoopSystem timerSystem; // Cached reference for removal

    /// <summary>
    /// Called automatically by Unity after assemblies are loaded.
    /// Injects TimerManager into the Update phase of the PlayerLoop.
    /// </summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    internal static void Initialize()
    {
        PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();

        // Register TimerManager into the Update loop
        if (!InsertTimerManager<Update>(ref currentPlayerLoop, 0))
        {
            Debug.LogWarning("Improved Timers not initialized, unable to register TimerManager into the Update loop.");
            return;
        }

        // Apply the modified PlayerLoop
        PlayerLoop.SetPlayerLoop(currentPlayerLoop);

#if UNITY_EDITOR
        // Handle editor play mode transitions to properly clean up systems
        EditorApplication.playModeStateChanged -= OnPlayModeState;
        EditorApplication.playModeStateChanged += OnPlayModeState;

        static void OnPlayModeState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                // Remove custom systems from PlayerLoop when exiting play mode
                PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
                RemoveTimerManager<Update>(ref currentPlayerLoop);

                PlayerLoop.SetPlayerLoop(currentPlayerLoop);

                // Clear static data to prevent stale references in editor
                TimerManager.Clear();
                DeathSystem.Clear();
            }
        }
#endif
    }

    #region TimerManager Integration
    /// <summary>
    /// Removes the TimerManager system from the specified PlayerLoop phase.
    /// </summary>
    /// <typeparam name="T">The PlayerLoop phase type (e.g., Update).</typeparam>
    /// <param name="loop">The PlayerLoop to modify.</param>
    private static void RemoveTimerManager<T>(ref PlayerLoopSystem loop)
    {
        PlayerLoopUtils.RemoveSystem<T>(ref loop, in timerSystem);
    }

    /// <summary>
    /// Inserts the TimerManager system into the specified PlayerLoop phase.
    /// </summary>
    /// <typeparam name="T">The PlayerLoop phase type (e.g., Update).</typeparam>
    /// <param name="loop">The PlayerLoop to modify.</param>
    /// <param name="index">Position within the phase's subsystem list.</param>
    /// <returns>True if insertion succeeded, false otherwise.</returns>
    private static bool InsertTimerManager<T>(ref PlayerLoopSystem loop, int index)
    {
        timerSystem = new PlayerLoopSystem()
        {
            type = typeof(TimerManager),
            updateDelegate = TimerManager.UpdateTimers,
            subSystemList = null
        };
        return PlayerLoopUtils.InsertSystem<T>(ref loop, in timerSystem, index);
    }
    #endregion
}
