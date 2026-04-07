using MonsterSystem;
using UnityEngine;

public class ChasingPlayerState : AnimatedState {
    [SerializeField] private BaseNavAIMonster monsterNavigation;
    [SerializeField] private float moveDistance = 3.5f;

    private void Awake() {
        if (this.monsterNavigation == null) this.monsterNavigation = this.transform.root.GetComponentInChildren<BaseNavAIMonster>();
    }

    public override void OnStateEnter() {
        //this.monsterNavigation.Agent.updatePosition = true;
        // Move the agent relative to its forward direction as it enters the chasing state
        this.monsterNavigation.EnableNavigation();

        // Might not need this anymore, if using the animation approache to fix the position problem, but leaving it here for now just in case.
        this.monsterNavigation.Agent.Warp(this.monsterNavigation.transform.position + (this.monsterNavigation.transform.forward * this.moveDistance));


        // The position of the animator dose not get change, use an animation instad to fix the problem.
        Animator animator = this.monsterNavigation.gameObject.transform.root.GetComponentInChildren<Animator>();
        //animator.gameObject.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        animator.gameObject.transform.position = Vector3.zero;
        animator.transform.localPosition = Vector3.zero;
        animator.gameObject.transform.localPosition = Vector3.zero;
        animator.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
        animator.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);


        base.OnStateEnter();
        this.monsterNavigation.DisableNavigation(); // Stop moving to check what the position ended up.
    }
}
