using UnityEngine;
using UnityEngine.AI;

public class MonsterAIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform playerTransform;

    [Header("Layer Masks")]
    [SerializeField] private LayerMask whatIsGround;
    [SerializeField] private LayerMask whatIsPlayer;

    [Header("Patrolling")]
    [SerializeField]
    [Tooltip("Maximum distance from origin the monster can patrol")]
    private float walkPointRange = 10f;

    [Header("Combat")]
    [SerializeField]
    [Tooltip("Time in seconds between attacks")]
    private float timeBetweenAttacks = 1.5f;

    [Header("Detection")]
    [SerializeField] private float sightRange = 15f;
    [SerializeField] private float attackRange = 2f;

    private Vector3 walkPoint;

    private bool isWalkPointSet;
    private bool hasAlreadyAttacked;
    private bool isPlayerInSightRange;
    private bool isPlayerInAttackRange;

    private void Awake()
    {
        if (this.agent == null)
        {
            this.agent = GetComponent<NavMeshAgent>();
        }
    }

    private void Update()
    {
        UpdatePlayerDetection();
        UpdateAIState();
    }


    private void UpdatePlayerDetection()
    {
        this.isPlayerInSightRange = Physics.CheckSphere(
            this.transform.position,
            this.sightRange,
            this.whatIsPlayer
        );

        this.isPlayerInAttackRange = Physics.CheckSphere(
            this.transform.position,
            this.attackRange,
            this.whatIsPlayer
        );
    }

    private void UpdateAIState()
    {
        if (this.isPlayerInAttackRange)
        {
            AttackPlayer();
            return;
        }

        if (this.isPlayerInSightRange)
        {
            ChasePlayer();
            return;
        }

        PatrolArea();
    }

    private void PatrolArea()
    {
        if (!this.isWalkPointSet)
        {
            SearchForWalkPoint();
        }

        if (this.isWalkPointSet)
        {
            this.agent.SetDestination(this.walkPoint);
        }

        if (Vector3.Distance(this.transform.position, this.walkPoint) < 0.5f)
        {
            this.isWalkPointSet = false;
        }
    }

    private void SearchForWalkPoint()
    {
        float randomX = Random.Range(-this.walkPointRange, this.walkPointRange);
        float randomZ = Random.Range(-this.walkPointRange, this.walkPointRange);

        Vector3 candidatePoint = new Vector3(
            this.transform.position.x + randomX,
            this.transform.position.y,
            this.transform.position.z + randomZ
        );

        if (Physics.Raycast(candidatePoint, Vector3.down, 2f, this.whatIsGround))
        {
            this.walkPoint = candidatePoint;
            this.isWalkPointSet = true;
        }
    }

    private void ChasePlayer()
    {
        if (this.playerTransform == null)
        {
            return;
        }

        this.agent.SetDestination(this.playerTransform.position);
    }

    private void AttackPlayer()
    {
        this.agent.SetDestination(this.transform.position);
        this.transform.LookAt(this.playerTransform);

        if (this.hasAlreadyAttacked)
        {
            return;
        }

        PerformAttack();

        this.hasAlreadyAttacked = true;
        Invoke(nameof(ResetAttackCooldown), this.timeBetweenAttacks);
    }

    private void PerformAttack()
    {
        
    }

    private void ResetAttackCooldown()
    {
        this.hasAlreadyAttacked = false;
    }
}
