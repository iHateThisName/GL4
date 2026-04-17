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

        static int batchIndex;
        static int batchSize = 5;

        // FUTURE: Per-sensor tick intervals
        // Each MonsterSensor could expose a float TickInterval (defaulting to 0.2s).
        // MonsterStateManager would group sensors into interval buckets:
        //   struct IntervalBucket { float interval; float elapsed; int batchIndex; MonsterSensor[] sensors; }
        // MonsterBootstrap would revert to a per-frame Update call (or GCD interval),
        // and each bucket would accumulate its own elapsed and fire independently.
        // Batching within a bucket works the same way batchIndex does today.
        // Main costs: manager tracks sensors directly (not controllers), isDirty rebuild
        // becomes per-bucket, and Bootstrap loses its fixed 0.2s gate.
        // Only worth doing if sensors have genuinely different perf needs
        // (e.g. expensive pathfinding sensor at 1s vs cheap proximity sensor at 0.1s).

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

        /// Called every 0.2s by MonsterBootstrap via PlayerLoop.
        public static void UpdateMonsters(float tickDelta)
        {
            if (activeCount == 0) return;
            
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
                var controller = sweep[i];
                if (controller != null && controller.isActiveAndEnabled)
                    controller.TickSensors(tickDelta);
            }

            batchIndex = (end >= sweepCount) ? 0 : end;
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

            batchIndex = 0;
            isDirty = false;
        }
    }
}
