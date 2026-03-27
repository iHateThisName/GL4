using MonsterSystem;
using UnityEngine;
using UnityEngine.AI;

public class IntruderApproachWindowState : AnimatedState {

    private const float animationStartPositionOffset = 1.08f;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private BaseNavAIMonster monsterNavigation;

    private void Awake() {
        if (this.agent == null) this.agent = this.GetComponentInParent<NavMeshAgent>();
        if (this.monsterNavigation == null) this.monsterNavigation = this.transform.root.GetComponentInChildren<BaseNavAIMonster>();
    }

    public override void OnStateEnter() {
        // Disable the navigation agent to prevent it from interfering with the animation.
        this.monsterNavigation.DisableNavigation();

        // Move the agent to the correct position for the animation to play properly.
        this.agent.Warp(this.agent.transform.position + (-this.agent.transform.right * animationStartPositionOffset));

        // Start playing the animation
        base.OnStateEnter();
    }

}
