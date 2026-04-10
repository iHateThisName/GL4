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
    }

    private System.Collections.IEnumerator NextFrame(Animator animator) {
        yield return null; // wait one frame
        this.monsterNavigation.Agent.Warp(this.monsterNavigation.transform.position + (this.monsterNavigation.transform.forward * this.moveDistance));
        Physics.SyncTransforms();
        yield return null;
        animator.applyRootMotion = true;

    }
}
