using System.Collections;
using MonsterSystem;
using UnityEngine;

public class IntruderIdleState : AnimatedState {

    public float countdownDuration = 3f; // Duration in seconds
    private float countdownTimer;

    public override void OnStateEnter() {
        base.OnStateEnter();
        StartCountdownCoroutine();
    }

    private void StartCountdownCoroutine() => StartCoroutine(CountdownCoroutine());

    private IEnumerator CountdownCoroutine() {
        countdownTimer = countdownDuration;

        while (countdownTimer > 0f) {
            countdownTimer -= Time.deltaTime;
            yield return null; // Wait for the next frame
        }

        OnCountdownFinished();
    }

    private void OnCountdownFinished() {
        Debug.Log("Countdown finished!");
        OnAnimationComplete();
    }
}
