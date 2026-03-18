using UnityEngine;
using TMPro; // Required for TextMeshPro

namespace MonsterSystem
{
    public class DollSensor : MonsterSensor
    {
        [Header("=== State Transitions ===")]
        [SerializeField] private MonsterState patientState;
        [SerializeField] private MonsterState impatientState;
        [SerializeField] private MonsterState aggressiveState;
        [SerializeField] private MonsterState attackState;

        [Header("=== Debug UI ===")]
        [SerializeField] private TMP_Text debugText;

        [HideInInspector]
        public Transform playerTransform;

        private float neglectTimer = 0f;
        private DollConfig config;
        private MonsterState lastTriggeredState;

        public override void Initialize(MonsterController owningMonster)
        {
            base.Initialize(owningMonster);
            config = owningMonster.GetConfig<DollConfig>();
            lastTriggeredState = patientState;

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("DollSensor: Could not find any GameObject with the 'Player' tag!");
            }
        }

        public override void OnTick(float tickDelta)
        {
            base.OnTick(tickDelta);

            if (playerTransform == null || config == null) return;

            float distance = Vector3.Distance(transform.position, playerTransform.position);

            // 1. Check for immediate kill condition if already aggressive
            if (controller.CurrentState == aggressiveState && distance <= config.attackDistance)
            {
                HandleStateTransition(attackState);
            }
            // 2. Handle Attention and Calming
            else if (distance <= config.attentionRadius)
            {
                neglectTimer = 0f; // Reset patience

                if (controller.CurrentState == impatientState)
                {
                    HandleStateTransition(patientState);
                }
            }
            // 3. Player is outside radius, build up neglect
            else
            {
                if (controller.CurrentState == patientState || controller.CurrentState == impatientState)
                {
                    neglectTimer += this.TickDelta;

                    if (neglectTimer >= (config.timeToImpatient + config.timeToAggressive))
                    {
                        HandleStateTransition(aggressiveState);
                    }
                    else if (neglectTimer >= config.timeToImpatient)
                    {
                        HandleStateTransition(impatientState);
                    }
                }
            }

            // Update the text at the end of the tick
            UpdateDebugUI();
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

        public void PetDoll()
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

            // Strip "Doll" and "State" from the class name for cleaner display (e.g., "DollPatientState" -> "Patient")
            string cleanStateName = controller.CurrentState.GetType().Name.Replace("Doll", "").Replace("State", "");
            string stateText = $"State: {cleanStateName}";
            string timerText = "";

            if (controller.CurrentState == patientState)
            {
                float timeLeft = config.timeToImpatient - neglectTimer;
                timerText = $"\nLosing Patience: {Mathf.Max(0, timeLeft):F1}s";
                debugText.color = Color.green;
            }
            else if (controller.CurrentState == impatientState)
            {
                float timeLeft = (config.timeToImpatient + config.timeToAggressive) - neglectTimer;
                timerText = $"\nAttacking In: {Mathf.Max(0, timeLeft):F1}s";
                debugText.color = Color.yellow;
            }
            else if (controller.CurrentState == aggressiveState)
            {
                float dist = Vector3.Distance(transform.position, playerTransform.position);
                timerText = $"\nChasing! Dist: {dist:F1}m";
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