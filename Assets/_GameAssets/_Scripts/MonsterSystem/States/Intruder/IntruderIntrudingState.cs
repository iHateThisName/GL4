using MonsterSystem;
using UnityEngine;
using UnityEngine.AI;

public class IntruderIntrudingState : AnimatedState {
    [SerializeField] private BaseNavAIMonster monsterNavigation;

    private void Awake() {
        if (this.monsterNavigation == null) this.monsterNavigation = this.transform.root.GetComponentInChildren<BaseNavAIMonster>();
    }
    public override void OnAnimationComplete() {
        base.OnAnimationComplete();
    }
}
