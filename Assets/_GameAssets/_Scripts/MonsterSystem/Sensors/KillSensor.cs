using MonsterSystem;
using UnityEngine;

public class KillSensor : MonsterSensor {

    [SerializeField] private BaseNavAIMonster navMonster;

    protected override void Subscribe() {
        base.Subscribe();
        this.navMonster.OnAttackPlayer += HandleAttackPlayer;
    }

    protected override void Unsubscribe() {
        base.Unsubscribe();
        this.navMonster.OnAttackPlayer -= HandleAttackPlayer;
    }

    private void HandleAttackPlayer() {
        navMonster.DisableNavigation();
        TriggerStateTransition();
    }
}
