using UnityEngine;

namespace MonsterSystem
{
    public static class MonsterStateManager
    {
        static MonsterController[] activeMonsters = new MonsterController[16];
        static int activeCount;
        static bool isDirty;

        static MonsterController[] sweep = new MonsterController[16];
        static int sweepCount;

        static float tickInterval = 0.2f;  // 5 ticks/sec
        static float elapsed;
        static int batchIndex;
        static int batchSize = 5;

        public static void Register(MonsterController controller)
        {
            if (activeCount >= activeMonsters.Length)
                System.Array.Resize(ref activeMonsters, activeCount * 2);
            activeMonsters[activeCount++] = controller;
            isDirty = true;
        }

        /// <summary>
        /// O(1) swap-back removal instead of O(n) List.Remove + shift.
        /// </summary>
        public static void Deregister(MonsterController controller)
        {
            for (int i = 0; i < activeCount; i++)
            {
                if (activeMonsters[i] == controller)
                {
                    activeMonsters[i] = activeMonsters[--activeCount];
                    activeMonsters[activeCount] = null;
                    isDirty = true;
                    return;
                }
            }
        }

        /// Called every frame by PlayerLoop (via MonsterBootstrap).
        public static void UpdateMonsters()
        {
            elapsed += Time.deltaTime;
            if (elapsed < tickInterval) return;

            float tickDelta = elapsed;
            elapsed = 0f;

            if (activeCount == 0) return;

            // Only copy when the list actually changed
            if (isDirty)
            {
                if (sweep.Length < activeCount)
                    System.Array.Resize(ref sweep, activeCount);
                System.Array.Copy(activeMonsters, sweep, activeCount);
                sweepCount = activeCount;
                isDirty = false;
            }

            int start = batchIndex;
            int end = Mathf.Min(start + batchSize, sweepCount);

            for (int i = start; i < end; i++)
            {
                var c = sweep[i];
                if (c != null && c.isActiveAndEnabled)
                    TickMonster(c, tickDelta);
            }

            batchIndex = (end >= sweepCount) ? 0 : end;
        }

        static void TickMonster(MonsterController controller, float tickDelta)
        {
            controller.TickSensors(tickDelta);

            if (controller.CurrentState != null)
                controller.CurrentState.OnStateTick(tickDelta);
        }

        /// Immediate imperative transition (called by states for event-driven changes).
        public static void RequestTransition(MonsterController controller, MonsterState targetState)
        {
            if (controller == null || targetState == null) return;
            controller.TransitionTo(targetState);
        }

        public static void RequestTransition<T>(MonsterController controller, MonsterState targetState, T context)
        {
            if (controller == null || targetState == null) return;
            controller.TransitionTo(targetState, context);
        }

        public static void Clear()
        {
            for (int i = 0; i < activeCount; i++)
                activeMonsters[i] = null;
            activeCount = 0;

            for (int i = 0; i < sweepCount; i++)
                sweep[i] = null;
            sweepCount = 0;

            elapsed = 0f;
            batchIndex = 0;
            isDirty = false;
        }
    }
}
