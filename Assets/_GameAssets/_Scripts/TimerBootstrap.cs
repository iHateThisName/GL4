using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

internal static class TimerBootstrap
{
    static PlayerLoopSystem timerSystem;
    static PlayerLoopSystem deathSystem;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    internal static void Initialize()
    {
        PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();

        if (!InsertTimerManager<Update>(ref currentPlayerLoop, 0))
        {
            Debug.LogWarning("Improved Timers not initialized, unable to register TimerManager into the Update loop.");
            return;
        }
        
        if (!InsertDeathManager<Update>(ref currentPlayerLoop, 0))
        {
            Debug.LogWarning("Death system not initialized, unable to register DeathSystem into the Update loop.");
            return;
        }

        PlayerLoop.SetPlayerLoop(currentPlayerLoop);

#if UNITY_EDITOR
        EditorApplication.playModeStateChanged -= OnPlayModeState;
        EditorApplication.playModeStateChanged += OnPlayModeState;

        static void OnPlayModeState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
                RemoveTimerManager<Update>(ref currentPlayerLoop);
                
                RemoveDeathManager<Update>(ref currentPlayerLoop);
                
                PlayerLoop.SetPlayerLoop(currentPlayerLoop);

                TimerManager.Clear();
                DeathSystem.Clear();
            }
        }
#endif
    }

    static void RemoveTimerManager<T>(ref PlayerLoopSystem loop)
    {
        PlayerLoopUtils.RemoveSystem<T>(ref loop, in timerSystem);
    }

    static bool InsertTimerManager<T>(ref PlayerLoopSystem loop, int index)
    {
        timerSystem = new PlayerLoopSystem()
        {
            type = typeof(TimerManager),
            updateDelegate = TimerManager.UpdateTimers,
            subSystemList = null
        };
        return PlayerLoopUtils.InsertSystem<T>(ref loop, in timerSystem, index);
    }
    
    static void RemoveDeathManager<T>(ref PlayerLoopSystem loop)
    {
        PlayerLoopUtils.RemoveSystem<T>(ref loop, in timerSystem);
    }

    static bool InsertDeathManager<T>(ref PlayerLoopSystem loop, int index)
    {
        deathSystem = new PlayerLoopSystem()
        {
            type = typeof(DeathSystem),
            subSystemList = null
        };
        return PlayerLoopUtils.InsertSystem<T>(ref loop, in deathSystem, index);
    }
}