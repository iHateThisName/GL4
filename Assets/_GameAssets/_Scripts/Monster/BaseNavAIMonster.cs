using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class BaseNavAIMonster : MonoBehaviour {

    [field: SerializeField] public string DebugInformation { get; private set; }

    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform player;
    [SerializeField] private Transform[] patrolPoints;

    [Header("Config")]
    [SerializeField] private MonsterTypeEnum monsterType;
    [SerializeField] private float tickRate = 2f; // How often the monster updates its behavior (in seconds)
    [SerializeField] private float attackRange = 0.5f;

    private Vector3 spawnPoint;
    private PlayerTemperatureSimulator.EnumLocationType currentLocation;
    private Action monsterNavigationLogic;
    private int currentPatrolIndex = 0;
    private bool isPlayerKilled = false;
    private enum MonsterTypeEnum { None, Stalker }

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
        if (this.agent == null) {
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
        this.currentLocation = type;
    }

    /// <summary>
    /// Coroutine that executes the selected monster behavior
    /// at fixed intervals defined by tickRate. Not necessary for the stalker behavior since it reacts to location changes, 
    /// but can be useful for other behaviors that require regular updates.
    /// </summary>
    private IEnumerator MonsterLogicCoroutine() {
        while (true) {
            // Check if the monster is in attack range of the player.
            float distanceToPlayer = Vector3.Distance(this.transform.position, this.player.position);
            if (distanceToPlayer <= this.attackRange) {
                AttackPlayer();
            }
            this.monsterNavigationLogic?.Invoke();

            yield return new WaitForSeconds(this.tickRate);
        }
    }

    /// <summary>
    /// Returns the appropriate behavior delegate
    /// based on the configured monster type.
    /// </summary>
    private Action MonsterLogicSelector() {
        switch (monsterType) {
            case MonsterTypeEnum.Stalker:
                return StalkerNavigationLogic;
            default:
                return () => { Debug.LogWarning($"The logic for the selected monster {this.monsterType} is missing."); };
        }
    }

    private void AttackPlayer() {
        if (this.isPlayerKilled) return; // Prevent multiple attack triggers if the player is already killed.
        this.isPlayerKilled = true;

        this.DebugInformation = "Monster is attacking the player!";
        // Implement attack logic here. trigger animation, reduce player health, etc.
        Debug.Log("Monster is attacking the player!");
        DeathSystem.KillPlayer(DeathSystem.DeathEvent.DeathReason.Monster, completelyRestart: false);
    }

    /// <summary>
    /// Implements the stalker behavior pattern for the monster.
    /// The stalker will pursue the player when they are in cold/outdoor locations,
    /// and retreat to its spawn point when the player enters warm/indoor areas.
    /// </summary>
    private void StalkerNavigationLogic() {
        // Check if player is outside.
        if (currentLocation == PlayerTemperatureSimulator.EnumLocationType.Cold) {
            this.agent.SetDestination(this.player.position);
            this.DebugInformation = $"Stalker is pursuing the player at {this.player.position}";

        } else {

            // Back off the player. Move towards spawn point.
            if (this.patrolPoints.Length == 0) {
                this.agent.SetDestination(this.spawnPoint);

            } else if (Vector3.Distance(this.transform.position, this.agent.destination) < this.attackRange) {
                // If the monster is close to the point, start patrolling between points.
                currentPatrolIndex = (currentPatrolIndex + 1) % this.patrolPoints.Length;
                this.agent.SetDestination(this.patrolPoints[currentPatrolIndex].position);

            } else {
                this.agent.SetDestination(this.patrolPoints[currentPatrolIndex].position);
            }
            this.DebugInformation = $"Stalker is idle moving towards {this.agent.destination}";
        }
    }
}
