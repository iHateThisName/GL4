using MonsterSystem;
using UnityEngine;

public class ChasingPlayerState : AnimatedState {
    [SerializeField] private BaseNavAIMonster monsterNavigation;
    [SerializeField] private float moveDistance = 1.55347f;

    private void Awake() {
        if (this.monsterNavigation == null) this.monsterNavigation = this.transform.root.GetComponentInChildren<BaseNavAIMonster>();
    }

    public override void OnStateEnter() {
        base.OnStateEnter();
        this.monsterNavigation.EnableNavigation();

        Animator animator = this.monsterNavigation.transform.root.GetComponentInChildren<Animator>();

        animator.applyRootMotion = false;
        StartCoroutine(NextFrame(animator));

        // Increase the monster travel speed by 100% when chasing the player
        this.monsterNavigation.Agent.speed *= 2f;

        // Increase the attack range by 150% when chasing the player
        this.monsterNavigation.AttackRange *= 2.5f;
    }

    private System.Collections.IEnumerator NextFrame(Animator animator) {
        yield return null; // wait one frame
        this.monsterNavigation.Agent.Warp(this.monsterNavigation.transform.position + (this.monsterNavigation.transform.forward * this.moveDistance));
        Physics.SyncTransforms();
        yield return null;
        animator.applyRootMotion = true;

    }
}
