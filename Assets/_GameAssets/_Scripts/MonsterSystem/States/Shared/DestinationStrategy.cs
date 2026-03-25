using System;
using UnityEngine;
using UnityEngine.AI;

namespace MonsterSystem
{
    /// <summary>
    /// Context passed to destination strategies for resolving a target position.
    /// </summary>
    public readonly struct DestinationContext
    {
        public readonly MonsterController Controller;
        public readonly Transform Target;
        public readonly Vector3[] Points;

        public DestinationContext(MonsterController controller, Transform target, Vector3[] points)
        {
            Controller = controller;
            Target = target;
            Points = points;
        }
    }

    /// <summary>
    /// Result returned by a destination strategy.
    /// </summary>
    public struct DestinationResult
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public bool HasRotation;

        public static DestinationResult At(Vector3 position)
        {
            return new DestinationResult { Position = position, Rotation = Quaternion.identity, HasRotation = false };
        }

        public static DestinationResult At(Vector3 position, Quaternion rotation)
        {
            return new DestinationResult { Position = position, Rotation = rotation, HasRotation = true };
        }
    }

    /// <summary>
    /// Abstract base for destination resolution strategies used by NavMeshMoveState.
    /// Each subclass decides how to pick a Vector3 destination — NavMeshMoveState handles the rest.
    /// </summary>
    [Serializable]
    public abstract class DestinationStrategy
    {
        public abstract DestinationResult ResolveDestination(in DestinationContext ctx);
    }

    // ─── Concrete Strategies ──────────────────────────────────────────

    /// <summary>
    /// Follows a single target (defaults to player from config if no context provided).
    /// </summary>
    [Serializable]
    public class FollowTargetStrategy : DestinationStrategy
    {
        public override DestinationResult ResolveDestination(in DestinationContext ctx)
        {
            Vector3 pos = ctx.Target != null ? ctx.Target.position : ctx.Controller.transform.position;
            return DestinationResult.At(pos);
        }
    }

    /// <summary>
    /// Flees away from the context target (e.g., flashlight).
    /// </summary>
    [Serializable]
    public class FleeStrategy : DestinationStrategy
    {
        [SerializeField] private float fleeDistance = 20f;

        public override DestinationResult ResolveDestination(in DestinationContext ctx)
        {
            if (ctx.Target == null) return DestinationResult.At(ctx.Controller.transform.position);

            Vector3 fleeDir = (ctx.Controller.transform.position - ctx.Target.position).normalized;
            Vector3 fleeTarget = ctx.Controller.transform.position + fleeDir * fleeDistance;

            if (NavMesh.SamplePosition(fleeTarget, out NavMeshHit hit, fleeDistance, NavMesh.AllAreas))
                return DestinationResult.At(hit.position);

            return DestinationResult.At(fleeTarget);
        }
    }

    /// <summary>
    /// Selects a destination from a set of predefined points using a nested PointSelector.
    /// </summary>
    [Serializable]
    public class GoToPointStrategy : DestinationStrategy
    {
        [SerializeReference] private PointSelector selector;

        public override DestinationResult ResolveDestination(in DestinationContext ctx)
        {
            if (selector == null)
            {
                Debug.LogWarning("[GoToPointStrategy] No point selector assigned!");
                return DestinationResult.At(ctx.Controller.transform.position);
            }

            return selector.SelectPoint(ctx.Points, in ctx);
        }
    }
}
