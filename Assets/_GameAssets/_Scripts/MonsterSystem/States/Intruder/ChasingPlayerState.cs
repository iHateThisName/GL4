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

        bool prevRootMotion = animator.applyRootMotion;
        animator.applyRootMotion = false;

        this.monsterNavigation.Agent.Warp(
            this.monsterNavigation.transform.position +
            (this.monsterNavigation.transform.forward * this.moveDistance)
        );

        Physics.SyncTransforms();

        animator.transform.localPosition = Vector3.zero;
        animator.transform.localRotation = Quaternion.identity;

        StartCoroutine(RestoreRootMotionNextFrame(animator, prevRootMotion));

        //this.monsterNavigation.DisableNavigation();
    }

    private System.Collections.IEnumerator RestoreRootMotionNextFrame(Animator animator, bool prevRootMotion) {
        yield return null; // wait one frame
        animator.applyRootMotion = prevRootMotion;
    }
}
