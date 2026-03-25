using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Initializes custom systems by injecting them into Unity's PlayerLoopSystem.
/// Runs automatically when assemblies are loaded, before any scene loads.
/// </summary>
internal static class SystemsBootstrap
{
    private static PlayerLoopSystem timerSystem;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    internal static void Initialize()
    {
        TimerManager.Initialize();

        PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();

        timerSystem = new PlayerLoopSystem()
        {
            type = typeof(TimerManager),
            updateDelegate = TimerManager.UpdateTimers,
            subSystemList = null
        };

        if (!PlayerLoopUtils.InsertSystem<Update>(ref currentPlayerLoop, in timerSystem, 0))
        {
            Debug.LogWarning("TimerManager not initialized, unable to register into the Update loop.");
            return;
        }

        PlayerLoop.SetPlayerLoop(currentPlayerLoop);

#if UNITY_EDITOR
        EditorApplication.playModeStateChanged -= OnPlayModeState;
        EditorApplication.playModeStateChanged += OnPlayModeState;
#endif
    }

#if UNITY_EDITOR
    private static void OnPlayModeState(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode)
        {
            PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
            PlayerLoopUtils.RemoveSystem<Update>(ref currentPlayerLoop, in timerSystem);
            PlayerLoop.SetPlayerLoop(currentPlayerLoop);

            TimerManager.Dispose();
            DeathSystem.Clear();
        }
    }
#endif
}
