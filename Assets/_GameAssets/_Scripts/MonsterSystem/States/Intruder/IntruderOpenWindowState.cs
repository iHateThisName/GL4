using MonsterSystem;
using UnityEngine;
using UnityEngine.AI;

public class IntruderOpenWindowState : AnimatedState {

    private const float animationStartPositionOffset = 1.08f;
    [SerializeField] private float offsetFromWindow = 0.5f;
    [SerializeField] private NavMeshAgent agent;

    private void Awake() {
        this.agent = this.GetComponentInParent<NavMeshAgent>();
    }

    // -0.567
    public override void OnStateEnter() {
        this.agent.Warp(this.agent.transform.position + (-this.agent.transform.right * animationStartPositionOffset));
        base.OnStateEnter();
    }

}
