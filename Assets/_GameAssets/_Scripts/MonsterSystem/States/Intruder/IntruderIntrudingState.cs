using MonsterSystem;
using UnityEngine;

public class IntruderIntrudingState : AnimatedState {
    [SerializeField] private BaseNavAIMonster monsterNavigation;
    [SerializeField] private MonsterSystem.LightSensor lightSensor;

    private void Awake() {
        if (this.monsterNavigation == null) this.monsterNavigation = this.transform.root.GetComponentInChildren<BaseNavAIMonster>();
        if (this.lightSensor == null) this.lightSensor = this.transform.root.GetComponentInChildren<MonsterSystem.LightSensor>();
    }
    public override void OnAnimationComplete() {
        base.OnAnimationComplete();
        this.lightSensor.gameObject.SetActive(false);
    }
}
