using UnityEngine;
using UnityEngine.AI;

namespace MonsterSystem
{
    /// <summary>
    /// State for NavMesh-based movement with multiple modes.
    ///
    /// FollowTarget: follows Config.PlayerTarget (or context if provided).
    /// GoToPoint:    navigates to points from a SO_NavMeshMoveConfig asset.
    /// AwayFromTarget: flees from a context Transform (e.g., flashlight from LightSensor).
    /// </summary>
    public class NavMeshMoveState : MonsterState, IStateWithContext<Transform>
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
            Random,
            Sequential,
            ClosestClosedWindow,
            RandomClosedWindow,
        }

        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private float baseSpeed = 3.5f;
        [SerializeField] private MoveMode mode;

        [Header("Target")]
        [SerializeField] private float targetThreshold = 0.5f;
        [SerializeField] private MonsterState arrivalState;

        [Header("AwayFromTarget")]
        [SerializeField] private float fleeDistance = 20f;

        [Header("GoToPoint")]
        [SerializeField] private SO_NavMeshMoveConfig moveConfig;
        [SerializeField] private PointSelection selectionStrategy;

        [Header("Window Selection")]
        [SerializeField] private SO_RuntimeReferences runtimeReferences;

        [Header("Audio")]
        [SerializeField] private AudioClip stateAudio;
        [SerializeField] private bool loopAudio = true;
        [SerializeField] [Range(0f, 1f)] private float audioVolume = 1f;

        // Resolved at runtime
        private Transform target;
        private Vector3[] resolvedPoints;
        private Vector3 lastSetDestination;
        private int sequentialIndex;

        public bool HasArrived { get; private set; }

        public void ReceiveContext(Transform context)
        {
            this.target = context;
        }

        public override void Initialize(MonsterController owningController)
        {
            base.Initialize(owningController);

            if (this.moveConfig != null && this.moveConfig.points != null && this.moveConfig.points.Length > 0)
            {
                this.resolvedPoints = new Vector3[this.moveConfig.points.Length];
                for (int i = 0; i < this.moveConfig.points.Length; i++)
                    this.resolvedPoints[i] = this.moveConfig.points[i].position;
            }
        }

        public override void OnStateEnter()
        {
            this.HasArrived = false;
            this.lastSetDestination = Vector3.positiveInfinity;

            if (this.agent != null)
                this.agent.isStopped = false;

            // FollowTarget resolves player from config if no context was provided
            if (this.target == null && this.mode == MoveMode.FollowTarget)
                this.target = this.controller.Config.PlayerTarget;

            switch (mode)
            {
                case MoveMode.GoToPoint:
                    SelectAndSetPointDestination();
                    break;
                case MoveMode.AwayFromTarget:
                    CalculateFleeDestination();
                    break;
            }

            if (this.stateAudio != null && this.controller.Audio != null)
                MonsterAudio.Play(this.controller.Audio, this.stateAudio, this.loopAudio, this.audioVolume);
        }

        public override void OnStateTick(float tickDelta)
        {
            if (this.agent == null) return;

            var nightOverride = this.controller.Config.GetOverrideForNight(this.controller.CurrentNight);
            this.agent.speed = this.baseSpeed * nightOverride.speedMultiplier;

            switch (mode)
            {
                case MoveMode.FollowTarget:
                    TickFollowTarget();
                    break;
                case MoveMode.AwayFromTarget:
                    TickAwayFromTarget();
                    break;
                case MoveMode.GoToPoint:
                    TickGoToPoint();
                    break;
            }

            if (this.HasArrived && this.arrivalState != null)
                RequestTransition(this.arrivalState);
        }

        public override void OnStateExit()
        {
            if (this.stateAudio != null && this.controller.Audio != null)
                MonsterAudio.Stop(this.controller.Audio);

            if (this.agent != null)
                this.agent.ResetPath();

            this.target = null;
        }

        private void TickFollowTarget()
        {
            if (this.target == null) return;

            Vector3 targetPos = this.target.position;

            if (Vector3.Distance(this.lastSetDestination, targetPos) > this.targetThreshold)
            {
                this.agent.SetDestination(targetPos);
                this.lastSetDestination = targetPos;
            }

            this.HasArrived = !this.agent.pathPending && this.agent.remainingDistance <= this.targetThreshold;
        }

        private void TickAwayFromTarget()
        {
            this.HasArrived = !this.agent.pathPending && this.agent.remainingDistance <= this.targetThreshold;
        }

        private void CalculateFleeDestination()
        {
            if (this.agent == null || this.target == null) return;

            Vector3 fleeDir = (this.controller.transform.position - this.target.position).normalized;
            Vector3 fleeTarget = this.controller.transform.position + fleeDir * this.fleeDistance;

            if (NavMesh.SamplePosition(fleeTarget, out NavMeshHit hit, this.fleeDistance, NavMesh.AllAreas))
                this.agent.SetDestination(hit.position);
        }

        private void TickGoToPoint()
        {
            if (this.resolvedPoints == null || this.resolvedPoints.Length == 0) return;

            this.HasArrived = !this.agent.pathPending && this.agent.remainingDistance <= this.targetThreshold;

            if (this.HasArrived && this.selectionStrategy == PointSelection.Sequential)
            {
                this.sequentialIndex = (this.sequentialIndex + 1) % this.resolvedPoints.Length;
                this.agent.SetDestination(this.resolvedPoints[this.sequentialIndex]);
                this.HasArrived = false;
            }
        }

        private void SelectAndSetPointDestination()
        {
            if (this.resolvedPoints == null || this.resolvedPoints.Length == 0) return;

            Vector3 destination;

            switch (this.selectionStrategy)
            {
                case PointSelection.ClosestToSelf:
                    destination = GetClosestToSelf();
                    break;
                case PointSelection.FurthestFromTarget:
                    destination = GetFurthestFromTarget();
                    break;
                case PointSelection.Sequential:
                    destination = this.resolvedPoints[this.sequentialIndex % this.resolvedPoints.Length];
                    break;
                case PointSelection.ClosestClosedWindow:
                    destination = GetClosestClosedWindow();
                    break;
                case PointSelection.RandomClosedWindow:
                    destination = GetRandomClosedWindow();
                    break;
                default:
                    destination = this.resolvedPoints[Random.Range(0, this.resolvedPoints.Length)];
                    break;
            }

            this.agent.SetDestination(destination);
        }

        private Vector3 GetClosestToSelf()
        {
            Vector3 closest = this.resolvedPoints[0];
            float closestDist = float.MaxValue;

            for (int i = 0; i < this.resolvedPoints.Length; i++)
            {
                float dist = Vector3.Distance(this.controller.transform.position, this.resolvedPoints[i]);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = this.resolvedPoints[i];
                }
            }
            return closest;
        }

        private Vector3 GetFurthestFromTarget()
        {
            if (this.target == null) return this.resolvedPoints[0];

            Vector3 furthest = this.resolvedPoints[0];
            float furthestDist = float.MinValue;

            for (int i = 0; i < this.resolvedPoints.Length; i++)
            {
                float dist = Vector3.Distance(this.target.position, this.resolvedPoints[i]);
                if (dist > furthestDist)
                {
                    furthestDist = dist;
                    furthest = this.resolvedPoints[i];
                }
            }
            return furthest;
        }

        private Vector3 GetClosestClosedWindow()
        {
            var closedWindows = this.runtimeReferences?.ClosedWindows;
            if (closedWindows == null || closedWindows.Length == 0)
            {
                Debug.LogWarning("[NavMeshMoveState] No closed windows found!", this);
                return this.controller.transform.position;
            }

            WindowController closest = closedWindows[0];
            float closestDist = float.MaxValue;

            for (int i = 0; i < closedWindows.Length; i++)
            {
                if (closedWindows[i].targetPosition == null) continue;

                float dist = Vector3.Distance(this.controller.transform.position, closedWindows[i].targetPosition.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = closedWindows[i];
                }
            }

            return closest.targetPosition != null ? closest.targetPosition.position : this.controller.transform.position;
        }

        private Vector3 GetRandomClosedWindow()
        {
            var closedWindows = this.runtimeReferences?.ClosedWindows;
            if (closedWindows == null || closedWindows.Length == 0)
            {
                Debug.LogWarning("[NavMeshMoveState] No closed windows found!", this);
                return this.controller.transform.position;
            }

            var window = closedWindows[Random.Range(0, closedWindows.Length)];
            return window.targetPosition != null ? window.targetPosition.position : this.controller.transform.position;
        }
    }
}
