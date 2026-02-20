using UnityEngine;
using UnityEngine.AI;

public class BaseNavAIMonster : MonoBehaviour {

    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform player;

    [Header("Layer Masks")]
    [SerializeField] private LayerMask whatIsGround;

    [Header("Config")]
    [SerializeField] private MonsterTypeEnum monsterType;

    private int outsideAreaMask = 8;

    private enum MonsterTypeEnum { None, Stalker }
    private void Start() {
        if (this.player == null) {
            this.player = GameObject.FindGameObjectWithTag("Player").transform.root;
        }

        switch (monsterType) {
            case MonsterTypeEnum.Stalker:

                // Check if player is outside (navmesh surface set to Outside)
                if (IsPlayerOutside()) {
                InvokeRepeating(nameof(MoveTowardsTarget), 0f, 1f); // Move towards the player every second

                } else {
                    // Back of the player. Move to spawn point.
                }


                    break;
        }
    }

    private void MoveTowardsTarget() {
        if (this.agent == null || this.player == null) return;
        this.agent.SetDestination(this.player.position);
    }

    private bool IsPlayerOutside() {

        NavMeshHit hit;

        if (NavMesh.SamplePosition(player.position, out hit, 2f, NavMesh.AllAreas)) {
            // Check if sampled position belongs to Outside area
            return (outsideAreaMask & (1 << hit.mask)) != 0;
        }

        return false;
    }


}
