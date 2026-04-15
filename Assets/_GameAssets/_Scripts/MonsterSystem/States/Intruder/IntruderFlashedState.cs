using MonsterSystem;
using UnityEngine;

public class IntruderFlashedState : AnimatedState {
    [SerializeField] private BaseNavAIMonster monsterNavigation;

    private void Awake() {
        if (this.monsterNavigation == null) this.monsterNavigation = this.transform.root.GetComponentInChildren<BaseNavAIMonster>();
    }
    public override void OnStateEnter() {
        monsterNavigation.DisableNavigation();
        base.OnStateEnter();
    }

    public override void OnAnimationComplete() {
        base.OnAnimationComplete();
        StartCoroutine(DelayedDestroy());
    }

     private System.Collections.IEnumerator DelayedDestroy() {
        yield return new WaitForSeconds(3f);
        GameObject go = transform.root.gameObject;
        Destroy(go);
    }
}
