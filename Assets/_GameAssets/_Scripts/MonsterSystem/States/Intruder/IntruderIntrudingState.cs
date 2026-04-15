using MonsterSystem;
using UnityEngine;

public class IntruderIntrudingState : AnimatedState {
    [SerializeField] private BaseNavAIMonster monsterNavigation;
    [SerializeField] private MonsterSystem.LightSensor lightSensor;
    [SerializeField] private Collider leftHandCollider;
    [SerializeField] private Collider rightHandCollider;

    private void Awake() {
        if (this.monsterNavigation == null) this.monsterNavigation = this.transform.root.GetComponentInChildren<BaseNavAIMonster>();
        if (this.lightSensor == null) this.lightSensor = this.transform.root.GetComponentInChildren<MonsterSystem.LightSensor>();
    }

    public override void OnStateEnter() {
        base.OnStateEnter();

        // Enable the colliders so the monster can interact with the window.
        this.leftHandCollider.enabled = true;
        this.rightHandCollider.enabled = true;

        // Disable smart update so the window will react to the intruder.
        this.monsterNavigation.GetCurrentTargetWindow().DisableSmartUpdate();
    }
    public override void OnAnimationComplete() {
        base.OnAnimationComplete();

        // Disable because the monster should not react anymore to the light.
        this.lightSensor.gameObject.SetActive(false);

        // Disable the colliders since we dont need to interact with the window anymore.
        this.leftHandCollider.enabled = false;
        this.rightHandCollider.enabled = false;
    }
}
