using MonsterSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BaseNavAIMonster : MonoBehaviour {

    [field: SerializeField] public string DebugInformation { get; private set; }

    [Header("References")]
    [field: SerializeField] public NavMeshAgent Agent { get; private set; }
    [SerializeField] private MonsterController monsterController;
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] patrolPoints;

    [Header("Sound")]
    [SerializeField] private AudioSource MonsterAudioSource;
    [SerializeField] private AudioClip killAudio;
    [SerializeField] private AudioClip flashedAudio;

    [Header("Config")]
    [SerializeField] private EnumMonsterType monsterType;
    [SerializeField] private float tickRate = 2f; // How often the monster updates its behavior (in seconds)
    [SerializeField] private float attackRange = 0.5f;

    [Header("Flee Behaviour")]
    [SerializeField] private float fleeDuration = 35f; // How long the monster flees after being hit by flashlight
    [SerializeField] private float fleeDistance = 20f; // How far the monster tries to flee 

    // Action events
    public Action OnAttackPlayer;

    // Nav
    private Vector3 spawnPoint;
    private int currentPatrolIndex = 0;
    private PlayerTemperatureSimulator.EnumLocationType currentPlayerLocation;
    private WindowController target; // For intruder behavior, the window the monster is trying to enter through.

    // Delegate for monster behavior logic. This will point to the appropriate function based on the monster type.
    private Action monsterNavigationLogic;

    // Flags
    private bool isPlayerKilled = false;
    private bool isFleeing = false;
    private bool isNavigationDisabled = false;

    // Flee
    private float fleeTimer = 0f;
    private Vector3 fleeDestination;

    [System.Serializable]
    public enum EnumMonsterType { None, Stalker, Munch, Intruder }

    #region Unity Lifecycle
    /// <summary>
    /// Subscribes to location change events when enabled.
    /// </summary>
    private void OnEnable() {
        PlayerTemperatureSimulator.OnLocationTypeChanged += HandleLocationChange;
    }

    /// <summary>
    /// Unsubscribes from location change events to prevent memory leaks.
    /// </summary>
    private void OnDisable() {
        PlayerTemperatureSimulator.OnLocationTypeChanged -= HandleLocationChange;
    }

    /// <summary>
    /// Initializes references, selects behavior strategy,
    /// and starts the ticking coroutine.
    /// </summary>
    private void Start() {
        if (this.player == null) {
            this.player = GameObject.FindGameObjectWithTag("Player").transform.root;
        }

        // Validate NavMeshAgent reference
        if (this.Agent == null) {
            Debug.LogError("NavMeshAgent reference is missing on BaseNavAIMonster. Please assign it in the inspector.");
        }

        if (this.patrolPoints.Length == 0) {
            Debug.LogWarning("No patrol points assigned to BaseNavAIMonster.");
        }

        // Set the spawn point to the monster's initial position
        this.spawnPoint = this.transform.parent.transform.position;

        // Initial check for location type
        HandleLocationChange(PlayerTemperatureSimulator.Instance.CurrentLocationType);

        // Select the appropriate logic function based on the monster type
        this.monsterNavigationLogic = MonsterLogicSelector();

        // Start the movement calculation coroutine. This will repeatedly call the selected monster logic function at the specified tick rate.
        StartCoroutine(MonsterLogicCoroutine());
    }

    #endregion

    /// <summary>
    /// Updates the location type when the environment changes.
    /// </summary>
    private void HandleLocationChange(PlayerTemperatureSimulator.EnumLocationType type) {
        this.currentPlayerLocation = type;

        // If the player is no longer in the Cold, stop the stalking audio.
        if (type != PlayerTemperatureSimulator.EnumLocationType.Cold) {
            UpdateStalkingAudio(false);
        }
    }

    /// <summary>
    /// Coroutine that executes the selected monster behavior
    /// at fixed intervals defined by tickRate. Not necessary for the stalker behavior since it reacts to location changes, 
    /// but can be useful for other behaviors that require regular updates.
    /// </summary>
    private IEnumerator MonsterLogicCoroutine() {
        while (true) {
            if (!this.isFleeing) CheckAttackRange();

            if (!this.isNavigationDisabled) this.monsterNavigationLogic?.Invoke();

            yield return new WaitForSeconds(this.tickRate);
        }
    }

    private void CheckAttackRange() {
        // Check if the monster is in attack range of the player.
        float distanceToPlayer = Vector3.Distance(this.transform.position, this.player.position);

        // Check if the monster type requires the player to be in a cold location for the attack to be valid.
        bool requiresCold = this.monsterType == EnumMonsterType.Stalker;
        bool isLocationValid = !requiresCold || this.currentPlayerLocation == PlayerTemperatureSimulator.EnumLocationType.Cold;

        if (distanceToPlayer <= this.attackRange && isLocationValid) {
            AttackPlayer();
        }
    }

    /// <summary>
    /// Returns the appropriate behavior delegate
    /// based on the configured monster type.
    /// </summary>
    private Action MonsterLogicSelector() {
        switch (this.monsterType) {
            case EnumMonsterType.Stalker:
                return StalkerNavigationLogic;
            case EnumMonsterType.Intruder:
                return IntruderNavigationLogic;
            default:
                return () => { Debug.LogWarning($"The logic for the selected monster {this.monsterType} is missing."); };
        }
    }

    private void IntruderNavigationLogic() {
        MonsterState currentState = this.monsterController.CurrentState;

        // check if the current animation state is the ChasingPlayerState then navigate towards the player directly. Skip other logic.
        if (currentState is ChasingPlayerState) {
            this.Agent.SetDestination(this.player.position);
            this.DebugInformation = $"Intruder is chasing the player at {this.player.position}";
            return; // Skip the window targeting logic
        }

        // Select a window to target if we don't have one or if the current target window is already open
        if (this.target == null || this.target.GetCurrentWindowState() == VRLever.EnumLeverState.Open) {
            // Get all windows that has the state closed
            List<WindowController> closedWindows = GameManager.Instance.GetClosedWindows();
            if (closedWindows.Count == 0) {
                Debug.LogWarning("Intruder monster cannot find any closed windows to target.");
                return;
            }
            // Randomly select one of the closed windows as the new target
            this.target = closedWindows[UnityEngine.Random.Range(0, closedWindows.Count)];
            // Start moving towards the target window's position
            //agent.SetDestination(this.target.TargetPosition.position);

            Vector3 approachePoint = this.target.TargetPosition.position - (-this.target.TargetPosition.right * 5f);
            Agent.SetDestination(approachePoint);
            this.DebugInformation = $"Intruder is targeting a window at {this.target.TargetPosition.position} and moving towards approache point at {approachePoint}";
        }


        // Check if target as been reached.
        if (!Agent.pathPending && Agent.velocity.sqrMagnitude == 0f) {
            // debug log the current position the pathendposition and the target position
            Vector3 targetPosition = this.target.TargetPosition.position;
            if (this.Agent.pathEndPosition.x == targetPosition.x && this.Agent.pathEndPosition.z == targetPosition.z) {
                DisableNavigation();
                // Reached Window target
                this.Agent.Warp(targetPosition); // Making sure the monster is exactly at the target position.
                this.Agent.gameObject.transform.rotation = this.target.TargetPosition.rotation; // Make sure the monster is rotated to match the window's rotation.

                // Tell the monster controller to start a diffrent animation state for opening the window.
                this.monsterController.TransitionTo(this.monsterController.GetMonsterState<IntruderApproachWindowState>());
                this.DebugInformation = "Intruder has reached the window and is now transitioning to open window state.";

            } else {
                // Reached approache point, now set destination to the window target position to move directly towards it.
                // This is to make sure the rotation of the monster is correct when it reaches the window, since the approache point is offset from the window position.
                this.Agent.SetDestination(targetPosition);
                this.DebugInformation = $"Intruder has reached the approach point and is now moving directly towards the window at {targetPosition}";
            }
        }

    }


    /// <summary>
    /// Implements the stalker behavior pattern for the monster.
    /// The stalker will pursue the player when they are in cold/outdoor locations,
    /// and retreat to its spawn point when the player enters warm/indoor areas.
    /// </summary>
    private void StalkerNavigationLogic() {
        if (this.isFleeing) {
            this.fleeTimer -= this.tickRate;
            if (this.fleeTimer <= 0f) this.isFleeing = false;
            return;
        }

        // Check if player is outside.
        if (currentPlayerLocation == PlayerTemperatureSimulator.EnumLocationType.Cold) {
            this.Agent.SetDestination(this.player.position);
            this.DebugInformation = $"Stalker is pursuing the player at {this.player.position}";

            //Audio
            UpdateStalkingAudio(true);

        } else {

            //Audio
            UpdateStalkingAudio(false);

            // Back off the player. Move towards spawn point.
            if (this.patrolPoints.Length == 0) {
                this.Agent.SetDestination(this.spawnPoint);

            } else if (Vector3.Distance(this.transform.position, this.patrolPoints[this.currentPatrolIndex].position) < this.attackRange) {
                // If the monster is close to the point, start patrolling between points.
                currentPatrolIndex = (currentPatrolIndex + 1) % this.patrolPoints.Length;
                this.Agent.SetDestination(this.patrolPoints[currentPatrolIndex].position);

            } else {
                this.Agent.SetDestination(this.patrolPoints[currentPatrolIndex].position);
            }
            this.DebugInformation = $"Stalker is idle moving towards {this.Agent.destination}";
        }
    }
    private void AttackPlayer() {
        if (this.isPlayerKilled) return; // Prevent multiple attack triggers if the player is already killed.
        this.isPlayerKilled = true;

        this.DebugInformation = $"{this.monsterType} is attacking the player!";

        //Audio
        //SoundEffectManager.Instance.PlaySoundFXClip(this.killAudio, transform, 0.75f); use a audio affordance instead.

        // Event trigger
        this.OnAttackPlayer?.Invoke();

        // Implement attack logic here. trigger animation, reduce player health, etc.
        //Debug.Log("Monster is attacking the player!");
        //DeathSystem.KillPlayer(DeathSystem.DeathEvent.DeathReason.Monster, completelyRestart: false);

    }

    private void UpdateStalkingAudio(bool isStalking) {
        if (MonsterAudioSource == null) return;

        if (isStalking) {
            // Only call Play if it's not already playing to avoid "stuttering" restarts
            if (!MonsterAudioSource.isPlaying) {
                MonsterAudioSource.Play();
            }
        } else {
            // Stop the sound if the monster is retreating/patrolling
            if (MonsterAudioSource.isPlaying) {
                MonsterAudioSource.Stop();
            }
        }
    }

    [ContextMenu("Simulate flashlight hit from front")]
    public void DebugFlaslgihtHitFront() => OnFlashlightHit(this.transform.position + this.transform.forward);
    [ContextMenu("Simulate flashlight hit from Origin")]
    public void DebugFlashlightHitOrigin() => OnFlashlightHit(Vector3.zero);

    /// <summary>
    /// Public method to be called when the monster is hit by a flashlight.
    /// Makes the stalker flee in the opposite direction from the light source.
    /// </summary>
    /// <param name="lightSourcePosition">The position of the flashlight/light source</param>
    public void OnFlashlightHit(Vector3 lightSourcePosition) {
        // Calculate flee direction (away from light source)
        Vector3 fleeDirection = (this.transform.position - lightSourcePosition).normalized;

        // Calculate flee destination
        this.fleeDestination = this.transform.position + (fleeDirection * this.fleeDistance);

        // Set fleeing state
        this.isFleeing = true;
        this.fleeTimer = this.fleeDuration;

        // Immediately set destination to flee
        this.Agent.SetDestination(this.fleeDestination);

        //Audio - stop stalking audio when hit by flashlight
        UpdateStalkingAudio(false);

        //Audio - play flashlight hit reaction sound
        SoundEffectManager.Instance.PlaySoundFXClip(audioClip: this.flashedAudio, spawmTransform: this.transform, volume: 0.75f, parentSpawnTransform: this.transform);

        Debug.Log($"Monster hit by flashlight! Fleeing from {lightSourcePosition} to {this.fleeDestination}");
    }

    public void SetPatrolPoints(Transform[] points) {
        this.patrolPoints = points;
    }

    public void DisableNavigation() {
        this.Agent.isStopped = true;
        this.Agent.ResetPath();

        this.Agent.updatePosition = false;
        this.Agent.updateRotation = false;

        this.isNavigationDisabled = true;
    }

    public void EnableNavigation() {
        this.Agent.isStopped = false;
        this.Agent.updatePosition = true;
        this.Agent.updateRotation = true;

        this.isNavigationDisabled = false;
    }
}
