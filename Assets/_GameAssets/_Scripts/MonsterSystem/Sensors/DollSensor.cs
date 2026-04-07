using UnityEngine;
using TMPro; // Required for TextMeshPro

namespace MonsterSystem
{
    public class DollSensor : MonsterSensor
    {
        [Header("=== State Transitions ===")]
        [SerializeField] private MonsterState patientState;
        [SerializeField] private MonsterState relocateState;
        [SerializeField] private MonsterState hidingState;
        [SerializeField] private MonsterState aggressiveState;
        [SerializeField] private MonsterState attackState;


        [Header("=== Debug UI ===")]
        [SerializeField] private TMP_Text debugText;

        private float neglectTimer = 0f;
        private DollConfig config;
        private MonsterState lastTriggeredState;

        public override void Initialize(MonsterController owningMonster)
        {
            base.Initialize(owningMonster);
            config = owningMonster.GetConfig<DollConfig>();
            lastTriggeredState = patientState;
        }

        public override void OnTick(float tickDelta)
        {
            base.OnTick(tickDelta);

            // Only tick the timer if she is in one of these three states
            if (controller.CurrentState == patientState || controller.CurrentState == relocateState || controller.CurrentState == hidingState)
            {
                neglectTimer += this.TickDelta;

                // 1. Check for Aggressive first (Timer has reached the absolute maximum)
                if (neglectTimer >= (config.timeToHiding + config.timeToAggressive))
                {
                    HandleStateTransition(aggressiveState);
                }
                // 2. Check for Relocate (Timer hit the first threshold, AND she is still in bed)
                else if (neglectTimer >= config.timeToHiding && controller.CurrentState == patientState)
                {
                    HandleStateTransition(relocateState);
                }
            }

            UpdateDebugUI();
        }

        public void ReducePatience(float delta)
        {
            this.neglectTimer += delta;
        }

        private void HandleStateTransition(MonsterState targetState)
        {
            if (targetState != null && targetState != lastTriggeredState)
            {
                lastTriggeredState = targetState;
                this.TriggerTransitionTo(targetState);
            }
        }

        public override void OnStateChanged()
        {
            base.OnStateChanged();
            lastTriggeredState = controller.CurrentState;
        }

        public void ResetTimer()
        {
            neglectTimer = 0f;

            if (controller.CurrentState != attackState)
            {
                HandleStateTransition(patientState);
            }
        }

        private void UpdateDebugUI()
        {
            if (debugText == null || controller.CurrentState == null) return;

            // Strip "Doll" and "State" from the class name for cleaner display
            string cleanStateName = controller.CurrentState.GetType().Name.Replace("Doll", "").Replace("State", "");
            string stateText = $"State: {cleanStateName}";
            string timerText = "";

            if (controller.CurrentState == patientState)
            {
                float timeLeft = config.timeToHiding - neglectTimer;
                timerText = $"\nHiding In: {Mathf.Max(0, timeLeft):F1}s";
                debugText.color = Color.green;
            }
            else if (controller.CurrentState == relocateState || controller.CurrentState == hidingState)
            {
                float timeLeft = (config.timeToHiding + config.timeToAggressive) - neglectTimer;
                timerText = $"\nAttacking In: {Mathf.Max(0, timeLeft):F1}s";

                // Use an orange color for the hiding phase to show rising tension
                debugText.color = new Color(1f, 0.5f, 0f);
            }
            else if (controller.CurrentState == aggressiveState)
            {
                //float dist = Vector3.Distance(transform.position, playerTransform.position);
                //timerText = $"\nChasing! Dist: {dist:F1}m";
                debugText.color = Color.red;
            }
            else if (controller.CurrentState == attackState)
            {
                timerText = "\nYOU ARE DEAD";
                debugText.color = Color.magenta;
            }

            debugText.text = stateText + timerText;
        }
    }
}