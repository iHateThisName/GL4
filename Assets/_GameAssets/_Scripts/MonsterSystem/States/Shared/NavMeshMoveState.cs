using UnityEngine;
using UnityEngine.AI;

namespace MonsterSystem
{
    public class NavMeshMoveState : MonsterState
    {
        public enum MoveMode
        {
            FollowTarget,
            AwayFromTarget,
            GoToPoint
        }

        public enum PointSelection
        {
            ClosestToSelf,
            FurthestFromTarget,
            Random
        }

        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private float baseSpeed = 3.5f;
        [SerializeField] private MoveMode mode;

        [Header("Target")]
        [SerializeField] private Transform target;
        [SerializeField] private float arrivalThreshold = 0.5f;

        [Header("FollowTarget")]
        [SerializeField] private float repositionThreshold = 1f;

        [Header("AwayFromTarget")]
        [SerializeField] private float fleeDistance = 20f;

        [Header("GoToPoint")]
        [SerializeField] private Transform[] points;
        [SerializeField] private PointSelection selectionStrategy;

        private Vector3 lastSetDestination;

        public Transform Target { get => target; set => target = value; }
        public bool HasArrived { get; private set; }

        public override void OnStateEnter(MonsterController controller)
        {
            HasArrived = false;
            lastSetDestination = Vector3.positiveInfinity;

            if (agent != null)
                agent.isStopped = false;

            if (mode == MoveMode.GoToPoint)
                SelectAndSetPointDestination(controller);
        }

        public override void OnStateTick(MonsterController controller, float tickDelta)
        {
            if (agent == null) return;

            var nightOverride = controller.Config.GetOverrideForNight(controller.CurrentNight);
            agent.speed = baseSpeed * nightOverride.speedMultiplier;

            switch (mode)
            {
                case MoveMode.FollowTarget:
                    TickFollowTarget();
                    break;
                case MoveMode.AwayFromTarget:
                    TickAwayFromTarget(controller);
                    break;
                case MoveMode.GoToPoint:
                    TickGoToPoint();
                    break;
            }
        }

        public override void OnStateExit(MonsterController controller)
        {
            if (agent != null)
                agent.ResetPath();
        }

        private void TickFollowTarget()
        {
            if (target == null) return;

            Vector3 targetPos = target.position;

            if (Vector3.Distance(lastSetDestination, targetPos) > repositionThreshold)
            {
                agent.SetDestination(targetPos);
                lastSetDestination = targetPos;
            }

            HasArrived = !agent.pathPending && agent.remainingDistance <= arrivalThreshold;
        }

        private void TickAwayFromTarget(MonsterController controller)
        {
            if (target == null) return;

            Vector3 fleeDir = (controller.transform.position - target.position).normalized;
            Vector3 fleeTarget = controller.transform.position + fleeDir * fleeDistance;

            if (NavMesh.SamplePosition(fleeTarget, out NavMeshHit hit, fleeDistance, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }

        private void TickGoToPoint()
        {
            HasArrived = !agent.pathPending && agent.remainingDistance <= arrivalThreshold;
        }

        private void SelectAndSetPointDestination(MonsterController controller)
        {
            if (points == null || points.Length == 0) return;

            Transform selected = null;

            switch (selectionStrategy)
            {
                case PointSelection.ClosestToSelf:
                    selected = GetClosestToSelf(controller);
                    break;
                case PointSelection.FurthestFromTarget:
                    selected = GetFurthestFromTarget();
                    break;
                case PointSelection.Random:
                    selected = points[Random.Range(0, points.Length)];
                    break;
            }

            if (selected != null)
                agent.SetDestination(selected.position);
        }

        private Transform GetClosestToSelf(MonsterController controller)
        {
            Transform closest = null;
            float closestDist = float.MaxValue;

            for (int i = 0; i < points.Length; i++)
            {
                if (points[i] == null) continue;
                float dist = Vector3.Distance(controller.transform.position, points[i].position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = points[i];
                }
            }

            return closest;
        }

        private Transform GetFurthestFromTarget()
        {
            if (target == null) return points[0];

            Transform furthest = null;
            float furthestDist = float.MinValue;

            for (int i = 0; i < points.Length; i++)
            {
                if (points[i] == null) continue;
                float dist = Vector3.Distance(target.position, points[i].position);
                if (dist > furthestDist)
                {
                    furthestDist = dist;
                    furthest = points[i];
                }
            }

            return furthest;
        }
    }
}
