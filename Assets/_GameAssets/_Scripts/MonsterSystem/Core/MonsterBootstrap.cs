using UnityEngine;
using UnityEngine.LowLevel;
using UnityEngine.PlayerLoop;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MonsterSystem
{
    internal static class MonsterBootstrap
    {
        static PlayerLoopSystem monsterSystem;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        internal static void Initialize()
        {
            PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();

            monsterSystem = new PlayerLoopSystem()
            {
                type = typeof(MonsterStateManager),
                updateDelegate = MonsterStateManager.UpdateMonsters,
                subSystemList = null
            };

            if (!PlayerLoopUtils.InsertSystem<Update>(ref currentPlayerLoop, in monsterSystem, 0))
            {
                Debug.LogWarning("MonsterStateManager not initialized, unable to register into the Update loop.");
                return;
            }

            PlayerLoop.SetPlayerLoop(currentPlayerLoop);

#if UNITY_EDITOR
            EditorApplication.playModeStateChanged -= OnPlayModeState;
            EditorApplication.playModeStateChanged += OnPlayModeState;
#endif
        }

#if UNITY_EDITOR
        static void OnPlayModeState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();
                PlayerLoopUtils.RemoveSystem<Update>(ref currentPlayerLoop, in monsterSystem);
                PlayerLoop.SetPlayerLoop(currentPlayerLoop);
                MonsterStateManager.Clear();
            }
        }
#endif
    }
}
