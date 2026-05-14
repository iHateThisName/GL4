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
        [SerializeField] private SO_TransformCollection moveConfig;

        [Header("Arrival")]
        [SerializeField] private float targetThreshold = 0.5f;
        [SerializeField] private MonsterState arrivalState;

        private Transform target;
        private Vector3[] resolvedPoints;
        private Vector3 lastSetDestination;
        private DestinationResult lastResult;
        private bool hasDestination;
        private bool hasArrived;

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
            
            // Trigger animation
            TriggerAffordances<AnimationAffordance>();
            
            this.hasArrived = false;
            this.hasDestination = false;

            if (this.agent != null && this.agent.isOnNavMesh)
            {
                this.agent.isStopped = false;
                if (!this.agent.stoppingDistance.Equals(this.targetThreshold))
                    this.agent.stoppingDistance = this.targetThreshold;
            }

            // Default to player if no context target was provided
            if (this.target == null)
            {
                this.target = this.controller.PlayerTarget;
                if (this.target == null)
                    Debug.LogWarning($"[NavMeshMoveState] {name}: No context target and PlayerTarget is null.");
            }
            
            // Resolve and set initial destination
            SetDestinationFromStrategy();

            // Trigger audio
            TriggerAffordances<AudioAffordance>();
        }

        protected override void OnTimerTick()
        {
            base.OnTimerTick();

            if (this.agent == null) return;

            // Apply night scaling
            var nightOverride = this.controller.GetOverrideForNight(this.controller.CurrentNight);
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
            
            // Arrival: path is computed, agent has stopped, and the path actually ended at the
            // intended destination (XZ only). Mirrors the check in BaseNavAIMonster so that stale
            // destinations (e.g. from VR locomotion moving the player mid-path) don't trigger arrival.
            if (this.hasDestination && !this.agent.pathPending && this.agent.velocity.sqrMagnitude == 0f)
            {
                Vector3 intendedDestination = this.strategy is FollowTargetStrategy && this.target != null
                    ? this.target.position
                    : this.lastSetDestination;

                Vector2 pathEnd2D = new Vector2(this.agent.pathEndPosition.x, this.agent.pathEndPosition.z);
                Vector2 target2D  = new Vector2(intendedDestination.x, intendedDestination.z);

                this.hasArrived = Vector2.Distance(pathEnd2D, target2D) <= this.targetThreshold;
            }
            else
            {
                this.hasArrived = false;
            }

            if (this.hasArrived)
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
            this.hasDestination = true;
        }

        private void CachePoints()
        {
            // Check strategy for its own config first, fall back to state-level config
            SO_TransformCollection config = this.moveConfig;

            if (config != null && config.points != null && config.points.Length > 0)
            {
                this.resolvedPoints = new Vector3[config.points.Length];
                for (int i = 0; i < config.points.Length; i++)
                    this.resolvedPoints[i] = config.points[i].position;
            }
        }
    }
}
