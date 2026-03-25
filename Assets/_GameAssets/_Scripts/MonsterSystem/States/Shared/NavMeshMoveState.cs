using UnityEngine;
using UnityEngine.AI;

namespace MonsterSystem
{
    /// <summary>
    /// Generic NavMesh movement state. Delegates destination selection to a DestinationStrategy.
    /// Handles agent control, arrival detection, speed scaling, and audio.
    /// </summary>
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

            if (this.agent != null)
                this.agent.isStopped = false;

            // Default to player if no context target was provided
            if (this.target == null)
                this.target = this.controller.Config.PlayerTarget;
            
            // Resolve and set initial destination
            SetDestinationFromStrategy();

            TriggerAffordances<AudioAffordance>();
        }

        protected override void OnTimerTick()
        {
            base.OnTimerTick();

            if (this.agent == null) return;

            // Apply night scaling
            var nightOverride = this.controller.Config.GetOverrideForNight(this.controller.CurrentNight);
            this.agent.speed = this.baseSpeed * nightOverride.speedMultiplier;

            // Only re-resolve destination for FollowTarget (tracks a moving target).
            // Other strategies resolve once on enter — their destination is fixed.
            if (this.strategy is FollowTargetStrategy && this.target != null)
            {
                Vector3 targetPos = this.target.position;
                if (Vector3.Distance(this.lastSetDestination, targetPos) > this.targetThreshold)
                {
                    this.agent.SetDestination(targetPos);
                    this.lastSetDestination = targetPos;
                }
            }
            
            // Check arrival — only when the agent has a valid, computed path.
            // remainingDistance is 0 before the path is computed, which would false-trigger arrival.
            this.HasArrived = !this.agent.pathPending
                              && this.agent.hasPath
                              && this.agent.remainingDistance <= this.targetThreshold;

            if (this.HasArrived)
            {
                // Apply arrival rotation if the strategy provided one
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

            if (this.agent != null)
                this.agent.ResetPath();

            this.target = null;
        }

        private void SetDestinationFromStrategy()
        {
            if (this.strategy == null || this.agent == null) return;

            var ctx = new DestinationContext(this.controller, this.target, this.resolvedPoints);
            this.lastResult = this.strategy.ResolveDestination(in ctx);

            this.agent.SetDestination(this.lastResult.Position);
            this.lastSetDestination = this.lastResult.Position;
        }

        private void CachePoints()
        {
            // Check strategy for its own config first, fall back to state-level config
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
