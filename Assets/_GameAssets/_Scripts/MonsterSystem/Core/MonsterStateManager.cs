using System.Collections.Generic;
using UnityEngine;

namespace MonsterSystem
{
    public static class MonsterStateManager
    {
        static readonly List<MonsterController> ACTIVE_MONSTERS = new();
        static readonly List<MonsterController> SWEEP = new();

        static float tickInterval = 0.2f;  // 5 ticks/sec
        static float elapsed = 0f;
        static int batchIndex = 0;
        static int batchSize = 5;  // Max monsters per tick cycle

        public static void Register(MonsterController controller) => ACTIVE_MONSTERS.Add(controller);
        public static void Deregister(MonsterController controller) => ACTIVE_MONSTERS.Remove(controller);

        /// Called every frame by PlayerLoop (via MonsterBootstrap).
        public static void UpdateMonsters()
        {
            elapsed += Time.deltaTime;
            if (elapsed < tickInterval) return;

            float tickDelta = elapsed;
            elapsed = 0f;

            // Snapshot active list to avoid mutation during iteration
            SWEEP.RefreshWith(ACTIVE_MONSTERS);
            int count = SWEEP.Count;
            if (count == 0) return;

            // Staggered batching: tick batchSize monsters per tick cycle,
            // cycling through the list across frames
            int start = batchIndex;
            int end = Mathf.Min(start + batchSize, count);

            for (int i = start; i < end; i++)
            {
                TickMonster(SWEEP[i], tickDelta);
            }

            batchIndex = (end >= count) ? 0 : end;
        }

        static void TickMonster(MonsterController controller, float tickDelta)
        {
            if (controller == null || !controller.isActiveAndEnabled) return;

            // 1. Tick sensors (refresh data before evaluating transitions)
            controller.TickSensors(tickDelta);

            // 2. Tick current state
            if (controller.CurrentState != null)
            {
                controller.CurrentState.OnStateTick(tickDelta);
            }
        }

        /// Immediate imperative transition (called by states for event-driven changes).
        public static void RequestTransition(MonsterController controller, MonsterState targetState)
        {
            if (controller == null || targetState == null) return;
            controller.TransitionTo(targetState);
        }

        /// <summary>
        /// Immediate imperative transition with typed context data.
        /// </summary>
        public static void RequestTransition<T>(MonsterController controller, MonsterState targetState, T context)
        {
            if (controller == null || targetState == null) return;
            controller.TransitionTo(targetState, context);
        }

        public static void Clear()
        {
            ACTIVE_MONSTERS.Clear();
            SWEEP.Clear();
            elapsed = 0f;
            batchIndex = 0;
        }
    }
}
