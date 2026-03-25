using System;
using UnityEngine;

namespace MonsterSystem
{
    /// <summary>
    /// Abstract base for point selection strategies used by GoToPointStrategy.
    /// Each subclass decides how to pick one Vector3 from an array of candidate points.
    /// </summary>
    [Serializable]
    public abstract class PointSelector
    {
        public abstract DestinationResult SelectPoint(Vector3[] points, in DestinationContext ctx);
    }

    // ─── Concrete Selectors ──────────────────────────────────────────

    [Serializable]
    public class RandomPointSelector : PointSelector
    {
        public override DestinationResult SelectPoint(Vector3[] points, in DestinationContext ctx)
        {
            if (points == null || points.Length == 0) return DestinationResult.At(ctx.Controller.transform.position);
            return DestinationResult.At(points[UnityEngine.Random.Range(0, points.Length)]);
        }
    }

    [Serializable]
    public class SequentialPointSelector : PointSelector
    {
        private int currentIndex;

        public override DestinationResult SelectPoint(Vector3[] points, in DestinationContext ctx)
        {
            if (points == null || points.Length == 0) return DestinationResult.At(ctx.Controller.transform.position);
            Vector3 point = points[currentIndex % points.Length];
            currentIndex = (currentIndex + 1) % points.Length;
            return DestinationResult.At(point);
        }
    }

    [Serializable]
    public class ClosestToSelfSelector : PointSelector
    {
        public override DestinationResult SelectPoint(Vector3[] points, in DestinationContext ctx)
        {
            if (points == null || points.Length == 0) return DestinationResult.At(ctx.Controller.transform.position);

            Vector3 selfPos = ctx.Controller.transform.position;
            Vector3 closest = points[0];
            float closestDist = float.MaxValue;

            for (int i = 0; i < points.Length; i++)
            {
                float dist = Vector3.Distance(selfPos, points[i]);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = points[i];
                }
            }
            return DestinationResult.At(closest);
        }
    }

    [Serializable]
    public class FurthestFromTargetSelector : PointSelector
    {
        public override DestinationResult SelectPoint(Vector3[] points, in DestinationContext ctx)
        {
            if (points == null || points.Length == 0) return DestinationResult.At(ctx.Controller.transform.position);
            if (ctx.Target == null) return DestinationResult.At(points[0]);

            Vector3 targetPos = ctx.Target.position;
            Vector3 furthest = points[0];
            float furthestDist = float.MinValue;

            for (int i = 0; i < points.Length; i++)
            {
                float dist = Vector3.Distance(targetPos, points[i]);
                if (dist > furthestDist)
                {
                    furthestDist = dist;
                    furthest = points[i];
                }
            }
            return DestinationResult.At(furthest);
        }
    }

    [Serializable]
    public class ClosestClosedWindowSelector : PointSelector
    {
        [SerializeField] private float excludeRadius = 1.5f;
        [SerializeField] private SO_WindowRegistryRef windowsRef;

        public override DestinationResult SelectPoint(Vector3[] points, in DestinationContext ctx)
        {
            var closedWindows = this.windowsRef?.ClosedWindows;
            if (closedWindows == null || closedWindows.Length == 0)
            {
                Debug.LogWarning("[ClosestClosedWindowSelector] No closed windows found!");
                return DestinationResult.At(ctx.Controller.transform.position);
            }

            Vector3 selfPos = ctx.Controller.transform.position;
            WindowController closest = null;
            float closestDist = float.MaxValue;

            for (int i = 0; i < closedWindows.Length; i++)
            {
                if (closedWindows[i].TargetPosition == null) continue;

                float dist = Vector3.Distance(selfPos, closedWindows[i].TargetPosition.position);
                if (dist < excludeRadius) continue;

                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = closedWindows[i];
                }
            }

            if (closest == null)
                closest = closedWindows[0];

            if (closest.TargetPosition == null)
                return DestinationResult.At(selfPos);

            return DestinationResult.At(closest.TargetPosition.position, closest.TargetPosition.rotation);
        }
    }

    [Serializable]
    public class RandomClosedWindowSelector : PointSelector
    {
        [SerializeField] private float excludeRadius = 1.5f;
        [SerializeField] private SO_WindowRegistryRef windowsRef;

        public override DestinationResult SelectPoint(Vector3[] points, in DestinationContext ctx)
        {
            var closedWindows = this.windowsRef?.ClosedWindows;
            if (closedWindows == null || closedWindows.Length == 0)
            {
                Debug.LogWarning("[RandomClosedWindowSelector] No closed windows found!");
                return DestinationResult.At(ctx.Controller.transform.position);
            }

            // Build candidate list excluding the window the monster is already at
            Vector3 selfPos = ctx.Controller.transform.position;
            int candidateCount = 0;

            for (int i = 0; i < closedWindows.Length; i++)
            {
                if (closedWindows[i].TargetPosition == null) continue;
                if (Vector3.Distance(selfPos, closedWindows[i].TargetPosition.position) < excludeRadius) continue;
                candidateCount++;
            }

            // If all windows are excluded, fall back to any closed window
            if (candidateCount == 0)
            {
                var fallback = closedWindows[0];
                return fallback.TargetPosition != null
                    ? DestinationResult.At(fallback.TargetPosition.position, fallback.TargetPosition.rotation)
                    : DestinationResult.At(selfPos);
            }

            // Pick a random candidate from the filtered set
            int pick = UnityEngine.Random.Range(0, candidateCount);
            int seen = 0;
            for (int i = 0; i < closedWindows.Length; i++)
            {
                if (closedWindows[i].TargetPosition == null) continue;
                if (Vector3.Distance(selfPos, closedWindows[i].TargetPosition.position) < excludeRadius) continue;

                if (seen == pick)
                {
                    return DestinationResult.At(closedWindows[i].TargetPosition.position, closedWindows[i].TargetPosition.rotation);
                }
                seen++;
            }

            return DestinationResult.At(selfPos);
        }
    }
}
