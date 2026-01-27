using UnityEngine;
using UnityEngine.AI;

public class MonsterController : MonoBehaviour
{
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Transform playerTransform;
    [SerializeField] LayerMask whatIsGround, whatIsPlayer;

    //Patroling
    Vector3 walkPoint;
    [SerializeField] bool walkPointSet;
    [SerializeField] float walkPointRange;

    //Attack
    [SerializeField] float timeBetweenAttacks;
    bool alreadyAttacked;

    //States
    [SerializeField] float sightRange, attackRange;
    bool playerInSightRange, playerInAttackRange;

    private void Awake()
    {
        playerTransform = GameObject.Find("Player").transform;
        agent = GetComponent<NavMeshAgent>();
    }

    private void Update()
    {
        playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);
        playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

        if (!playerInSightRange && !playerInAttackRange) Patroling();
        if (playerInSightRange && !playerInAttackRange) ChasePlayer();
        if (playerInAttackRange && playerInSightRange) AttackPlayer();
    }

    private void Patroling()
    {
        if (!walkPointSet) 
        {
            SearchWalkPoint();
        }
        
        if (walkPointSet)
        {
            agent.SetDestination(walkPoint);
        }

        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        //Walk point reached
        if (distanceToWalkPoint.magnitude < 0.5f)
        {
            walkPointSet = false;
        }
    }

    private void SearchWalkPoint()
    {
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
        {
            walkPointSet = true;
        }
    }

    private void ChasePlayer()
    {
        agent.SetDestination(playerTransform.position);

    }

    private void AttackPlayer()
    {
        agent.SetDestination(transform.position);

        transform.LookAt(playerTransform);

        if (!alreadyAttacked)
        {
            ///Attack code here

            alreadyAttacked = true;
            Invoke(nameof(ResetAttack), timeBetweenAttacks);
        }
    }

    private void ResetAttack()
    {
        alreadyAttacked = false;
    }
}
