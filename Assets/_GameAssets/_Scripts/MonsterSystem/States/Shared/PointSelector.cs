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
}
