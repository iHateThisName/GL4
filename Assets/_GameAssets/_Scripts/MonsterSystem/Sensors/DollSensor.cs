using UnityEngine;
using TMPro;

namespace MonsterSystem
{
    public class DollSensor : MonsterSensor
    {
        [Header("=== State Transitions ===")]
        [SerializeField] private MonsterState patientState;
        [SerializeField] private MonsterState relocateState; // Teleports to hiding spot
        [SerializeField] private MonsterState hidingState;

        [Tooltip("The setup state that handles the jump scare and NavMesh teleport before chasing.")]
        [SerializeField] private MonsterState aggressiveSetupState; // NEW: The bridge state!

        [SerializeField] private MonsterState aggressiveState; // The actual chase state
        [SerializeField] private MonsterState attackState;

        [Header("=== Config ===")]
        [SerializeField] private float timeToAggressive = 20f;
        [SerializeField] private float timeToHiding = 20f;

        [Header("=== Debug UI ===")]
        [SerializeField] private TMP_Text debugText;

        private float neglectTimer = 0f;
        private MonsterState lastTriggeredState;

        public override void Initialize(MonsterController owningMonster)
        {
            base.Initialize(owningMonster);
            this.lastTriggeredState = this.patientState;
        }

        public override void OnTick(float tickDelta)
        {
            base.OnTick(tickDelta);

            // Only tick the timer if she is in one of these three states
            if (this.controller.CurrentState == this.patientState || this.controller.CurrentState == this.relocateState || this.controller.CurrentState == this.hidingState)
            {
                this.neglectTimer += this.TickDelta;

                // 1. Check for Aggressive first (Timer has reached the absolute maximum)
                if (this.neglectTimer >= (this.timeToHiding + this.timeToAggressive))
                {
                    // Trigger the Setup/Jump Scare state instead of the direct chase!
                    this.HandleStateTransition(this.aggressiveSetupState);
                }
                // 2. Check for Relocate (Timer hit the first threshold, AND she is still in bed)
                else if (this.neglectTimer >= this.timeToHiding && this.controller.CurrentState == this.patientState)
                {
                    this.HandleStateTransition(this.relocateState);
                }
            }

            this.UpdateDebugUI();
        }

        public void ReducePatience(float delta)
        {
            this.neglectTimer += delta;
        }

        private void HandleStateTransition(MonsterState targetState)
        {
            if (targetState != null && targetState != this.lastTriggeredState)
            {
                this.lastTriggeredState = targetState;
                this.TriggerTransitionTo(targetState);
            }
        }

        public override void OnStateChanged()
        {
            base.OnStateChanged();
            this.lastTriggeredState = this.controller.CurrentState;
        }

        public void ResetTimer()
        {
            this.neglectTimer = 0f;

            if (this.controller.CurrentState != this.attackState)
            {
                this.HandleStateTransition(this.patientState);
            }
        }

        private void UpdateDebugUI()
        {
            if (this.debugText == null || this.controller.CurrentState == null)
            {
                return;
            }

            // Strip "Doll" and "State" from the class name for cleaner display
            string cleanStateName = this.controller.CurrentState.GetType().Name.Replace("Doll", "").Replace("State", "");
            string stateText = $"State: {cleanStateName}";
            string timerText = "";

            if (this.controller.CurrentState == this.patientState)
            {
                float timeLeft = this.timeToHiding - this.neglectTimer;
                timerText = $"\nHiding In: {Mathf.Max(0, timeLeft):F1}s";
                this.debugText.color = Color.green;
            }
            else if (this.controller.CurrentState == this.relocateState || this.controller.CurrentState == this.hidingState)
            {
                float timeLeft = (this.timeToHiding + this.timeToAggressive) - this.neglectTimer;
                timerText = $"\nAttacking In: {Mathf.Max(0, timeLeft):F1}s";

                // Use an orange color for the hiding phase to show rising tension
                this.debugText.color = new Color(1f, 0.5f, 0f);
            }
            else if (this.controller.CurrentState == this.aggressiveSetupState)
            {
                timerText = "\nTELEPORTING...";
                this.debugText.color = Color.yellow;
            }
            else if (this.controller.CurrentState == this.aggressiveState)
            {
                timerText = "\nCHASING!";
                this.debugText.color = Color.red;
            }
            else if (this.controller.CurrentState == this.attackState)
            {
                timerText = "\nYOU ARE DEAD";
                this.debugText.color = Color.magenta;
            }

            this.debugText.text = stateText + timerText;
        }
    }
}