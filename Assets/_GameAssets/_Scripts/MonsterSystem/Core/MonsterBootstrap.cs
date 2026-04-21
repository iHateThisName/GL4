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
        const float tickInterval = 0.2f;
        static float elapsed;

        static void Tick()
        {
            elapsed += Time.deltaTime;
            if (elapsed < tickInterval) return;
            float tickDelta = elapsed;
            elapsed = 0f;
            MonsterStateManager.UpdateMonsters(tickDelta);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        internal static void Initialize()
        {
            elapsed = 0f;
            PlayerLoopSystem currentPlayerLoop = PlayerLoop.GetCurrentPlayerLoop();

            monsterSystem = new PlayerLoopSystem()
            {
                type = typeof(MonsterStateManager),
                updateDelegate = Tick,
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
                elapsed = 0f;
                MonsterStateManager.Clear();
            }
        }
#endif
    }
}
