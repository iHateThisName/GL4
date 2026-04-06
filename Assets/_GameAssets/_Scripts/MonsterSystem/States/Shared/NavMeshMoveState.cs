using UnityEngine;
using UnityEngine.AI;

namespace MonsterSystem
{
    public class NavMeshMoveState : MonsterStateWithTimer, IStateWithContext<Transform>
    {
        [SerializeField] private NavMeshAgent agent;
        [SerializeField] private float baseSpeed = 3.5f;

        [Header("Destination")]
        [SerializeReference] private DestinationStrategy strategy;
        [SerializeField] private SO_NavMeshMoveConfig moveConfig;

        [Header("Arrival")]
        [SerializeField] private float targetThreshold = 0.5f;
        [SerializeField] private MonsterState arrivalState;

        private Transform target;
        private Vector3[] resolvedPoints;
        private Vector3 lastSetDestination;
        private DestinationResult lastResult;

        public bool HasArrived { get; private set; }

        public void ReceiveContext(Transform context)
        {
            this.target = context;
        }

        public override void Initialize(MonsterController owningController)
        {
            base.Initialize(owningController);
            CachePoints();
        }

        public override void OnStateEnter()
        {
            base.OnStateEnter();

            TriggerAffordances<AnimationAffordance>();

            this.HasArrived = false;
            this.lastSetDestination = Vector3.positiveInfinity;

            // SAFETY CHECK ADDED: Only resume if the agent is ready
            if (this.agent != null && this.agent.isActiveAndEnabled && this.agent.isOnNavMesh)
            {
                this.agent.isStopped = false;
            }

            if (this.target == null)
                this.target = this.controller.Config.PlayerTarget;

            SetDestinationFromStrategy();

            TriggerAffordances<AudioAffordance>();
        }

        protected override void OnTimerTick()
        {
            base.OnTimerTick();

            // SAFETY CHECK: Ensure the agent is active and on the grid before doing anything
            if (this.agent == null || !this.agent.isActiveAndEnabled || !this.agent.isOnNavMesh) return;

            // THE FIX: Take the brakes off if they were stuck on from the frame delay
            if (this.agent.isStopped)
            {
                this.agent.isStopped = false;
            }

            var nightOverride = this.controller.Config.GetOverrideForNight(this.controller.CurrentNight);
            this.agent.speed = this.baseSpeed * nightOverride.speedMultiplier;

            if (this.strategy is FollowTargetStrategy && this.target != null)
            {
                Vector3 targetPos = this.target.position;
                if (Vector3.Distance(this.lastSetDestination, targetPos) > this.targetThreshold)
                {
                    this.agent.SetDestination(targetPos);
                    this.lastSetDestination = targetPos;
                }
            }
            else if (!this.agent.hasPath && !this.agent.pathPending && this.lastSetDestination != Vector3.positiveInfinity)
            {
                this.agent.SetDestination(this.lastResult.Position);
            }

            this.HasArrived = !this.agent.pathPending
                              && this.agent.hasPath
                              && this.agent.remainingDistance <= this.targetThreshold;

            if (this.HasArrived)
            {
                if (this.lastResult.HasRotation)
                    this.controller.transform.rotation = this.lastResult.Rotation;

                if (this.arrivalState != null)
                {
                    RequestTransition(this.arrivalState);
                }
            }
        }

        public override void OnStateExit()
        {
            base.OnStateExit();

            StopAffordances();

            if (this.agent != null && this.agent.isActiveAndEnabled && this.agent.isOnNavMesh)
                this.agent.ResetPath();

            this.target = null;
        }

        private void SetDestinationFromStrategy()
        {
            if (this.strategy == null || this.agent == null) return;

            var ctx = new DestinationContext(this.controller, this.target, this.resolvedPoints);
            this.lastResult = this.strategy.ResolveDestination(in ctx);

            // SAFETY CHECK ADDED
            if (this.agent.isActiveAndEnabled && this.agent.isOnNavMesh)
            {
                this.agent.SetDestination(this.lastResult.Position);
                this.lastSetDestination = this.lastResult.Position;
            }
        }

        private void CachePoints()
        {
            SO_NavMeshMoveConfig config = this.moveConfig;

            if (config != null && config.points != null && config.points.Length > 0)
            {
                this.resolvedPoints = new Vector3[config.points.Length];
                for (int i = 0; i < config.points.Length; i++)
                    this.resolvedPoints[i] = config.points[i].position;
            }
        }
    }
}