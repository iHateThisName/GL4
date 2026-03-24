using UnityEngine;
using UnityEngine.AI;
using TMPro; // Required for TextMeshPro

[RequireComponent(typeof(NavMeshAgent))]
public class TheDoll : MonoBehaviour
{
    public enum DollState
    {
        Inactive,
        Patient,
        Impatient,
        Aggressive,
        Attack
    }

    [Header("Core Settings")]
    public DollState currentState = DollState.Patient;
    public Transform playerTransform;
    private NavMeshAgent navAgent;

    [Header("Attention & Distance Settings")]
    [Tooltip("How close the player needs to stand to calm the doll.")]
    public float attentionRadius = 3.0f;
    [Tooltip("Distance required to trigger an attack when aggressive.")]
    public float attackDistance = 1.5f;

    [Header("Timers")]
    [Tooltip("Seconds before the doll gets impatient when ignored.")]
    public float timeToImpatient = 5.0f;
    [Tooltip("Seconds before the impatient doll stands up and becomes aggressive.")]
    public float timeToAggressive = 5.0f;

    [Header("UI / Debugging")]
    [Tooltip("Reference to the in-game TextMeshPro component.")]
    public TMP_Text debugText;

    private float attentionTimer = 0f;

    private void Start()
    {
        navAgent = GetComponent<NavMeshAgent>();

        // Make sure the doll stays completely still to start
        navAgent.isStopped = true;
    }

    private void Update()
    {
        if (currentState == DollState.Inactive)
        {
            UpdateDebugUI();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        HandleAttentionTimers(distanceToPlayer);
        HandleStateBehaviors(distanceToPlayer);

        // Update the text every frame
        UpdateDebugUI();
    }

    private void HandleAttentionTimers(float distanceToPlayer)
    {
        // If the player is within the attention radius, keep the doll calm.
        if (distanceToPlayer <= attentionRadius && currentState != DollState.Aggressive && currentState != DollState.Attack)
        {
            ResetToPatient();
            return;
        }

        // If the player is outside the radius, the timer starts ticking.
        if (currentState == DollState.Patient || currentState == DollState.Impatient)
        {
            attentionTimer += Time.deltaTime;

            if (currentState == DollState.Patient && attentionTimer >= timeToImpatient)
            {
                TransitionToState(DollState.Impatient);
            }
            else if (currentState == DollState.Impatient && attentionTimer >= (timeToImpatient + timeToAggressive))
            {
                TransitionToState(DollState.Aggressive);
            }
        }
    }

    private void HandleStateBehaviors(float distanceToPlayer)
    {
        switch (currentState)
        {
            case DollState.Aggressive:
                // The doll is off the chair and chasing the player
                navAgent.isStopped = false;
                navAgent.SetDestination(playerTransform.position);

                // Check if close enough to attack
                if (distanceToPlayer <= attackDistance)
                {
                    TransitionToState(DollState.Attack);
                }
                break;

            case DollState.Attack:
                // Stop moving and trigger the kill sequence
                navAgent.isStopped = true;

                // TODO: Trigger kill screen / player death logic here
                break;
        }
    }

    private void TransitionToState(DollState newState)
    {
        if (currentState == newState) return;

        currentState = newState;
        Debug.Log($"The Doll is now: {currentState}");
    }

    private void ResetToPatient()
    {
        if (currentState == DollState.Patient) return;

        currentState = DollState.Patient;
        attentionTimer = 0f;
        navAgent.isStopped = true;

        Debug.Log("The Doll is relaxing...");
    }

    public void PetDoll()
    {
        Debug.Log("The Doll was petted!");

        // Instantly calm the doll down, resetting timers and stopping movement
        currentState = DollState.Patient;
        attentionTimer = 0f;
        navAgent.isStopped = true;
    }

    private void UpdateDebugUI()
    {
        // Don't do anything if we haven't assigned a text component
        if (debugText == null) return;

        string stateText = $"State: {currentState}";
        string timerText = "";

        // Calculate time remaining based on the current state and build the display string
        if (currentState == DollState.Patient)
        {
            float timeLeft = timeToImpatient - attentionTimer;
            timerText = $"\nLosing Patience In: {Mathf.Max(0, timeLeft):F1}s";
            debugText.color = Color.green;
        }
        else if (currentState == DollState.Impatient)
        {
            float timeLeft = (timeToImpatient + timeToAggressive) - attentionTimer;
            timerText = $"\nAttacking In: {Mathf.Max(0, timeLeft):F1}s";
            debugText.color = Color.yellow;
        }
        else if (currentState == DollState.Aggressive)
        {
            float dist = Vector3.Distance(transform.position, playerTransform.position);
            timerText = $"\nChasing! Distance: {dist:F1}m";
            debugText.color = Color.red;
        }
        else if (currentState == DollState.Attack)
        {
            timerText = "\nYOU ARE DEAD";
            debugText.color = Color.magenta;
        }

        // Apply the combined text
        debugText.text = stateText + timerText;
    }
}