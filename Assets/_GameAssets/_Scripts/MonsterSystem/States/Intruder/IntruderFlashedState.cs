using MonsterSystem;
using UnityEngine;

public class IntruderFlashedState : AnimatedState {
    [SerializeField] private BaseNavAIMonster monsterNavigation;
    [SerializeField] private GameObject intruderModel; // The child gameobject of the "Model" gameobject
    [SerializeField] private float respawnDelay = 10f;

    private void Awake() {
        if (this.monsterNavigation == null) this.monsterNavigation = this.transform.root.GetComponentInChildren<BaseNavAIMonster>();
    }
    public override void OnStateEnter() {
        monsterNavigation.DisableNavigation();
        base.OnStateEnter();
        TriggerAffordances<VfxAffordance>();
    }

    public override void OnAnimationComplete() {
        base.OnAnimationComplete();
        //StartCoroutine(DelayedDestroy());
        StartCoroutine(DelayedRespawned());
    }

     private System.Collections.IEnumerator DelayedDestroy() {
        yield return new WaitForSeconds(1.5f);
        GameObject go = transform.root.gameObject;
        Destroy(go);
    }

    private System.Collections.IEnumerator DelayedRespawned() {
        this.monsterNavigation.DisableNavigation(); 
        this.monsterNavigation.IsFleeing = true; // Avoids the monster from killing the player
        yield return new WaitForSeconds(0.5f); // Time to show the animations of the monster being flashed

        this.intruderModel.SetActive(false); // Hide the model

        // Respawn the monster after a delay, resetting its position and re-enabling the model and navigation
        yield return new WaitForSeconds(this.respawnDelay);
        this.monsterNavigation.ResetMonster();
        this.intruderModel.SetActive(true);
        RequestTransition(this.nextState);

    }
}
