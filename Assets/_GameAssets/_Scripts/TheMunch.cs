using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using TMPro;
using System.Collections;

public class TheMunch : MonoBehaviour
{
    [Header("Animation")]
    [SerializeField] private Animator monsterAnimator;
    [SerializeField] private string munchTriggerName = "Munch";
    [SerializeField] private string returnTriggerName = "Return";
    [SerializeField] private string rejectTriggerName = "Reject";

    [Header("Hunger System Settings")]
    [Tooltip("Maximum fullness. Starts at 60.")]
    [SerializeField] private float maxSatiety = 60f;
    [SerializeField] private float currentSatiety = 60f;
    [SerializeField] private float flatSatietyGain = 25f;

    [Header("Audio Settings")]
    [Tooltip("Assign an AudioSource on The Munch to play sounds.")]
    [SerializeField] private AudioSource audioSource;
    [Tooltip("Assign the sound effect for when the monster gets hungry (loops).")]
    [SerializeField] private AudioClip hungrySound;
    [Tooltip("Assign the warning sound for the Angry state (loops).")]
    [SerializeField] private AudioClip angryWarningSound;
    [Tooltip("Assign the jumpscare sound effect for the Kill state (plays once).")]
    [SerializeField] private AudioClip killJumpscareSound;
    [Tooltip("Assign the eat sound effect for the Munch eating (plays once).")]
    [SerializeField] private AudioClip eatSound;

    [Header("Interaction Settings")]
    [Range(0, 10)]
    [SerializeField] private float maxAcceptableVelocity = 2.0f;

    [Header("Rejection Settings")]
    [Range(1, 20)]
    [SerializeField] private float throwForce = 5f;

    [Header("Debug UI")]
    [Tooltip("Assign a TextMeshPro (UI or 3D) component to see live updates.")]
    [SerializeField] private TMP_Text debugText;

    public Vector3 ThrowDirection = new Vector3(0, 1, 1);

    private EnumMunchState currentState;
    private BaseNavAIMonster.MonsterTypeEnum monsterType = BaseNavAIMonster.MonsterTypeEnum.Munch;

    public enum EnumMunchState
    {
        NotHungry,
        Hungry,
        Angry,
        Kill
    }

    private void Start()
    {
        this.currentSatiety = this.maxSatiety;

        // Force the initial state to run so the monster hides/retracts immediately on scene load
        this.ChangeState(EnumMunchState.NotHungry);
    }

    private void Update()
    {
        // Stop processing hunger if the monster has already caught the player
        if (this.currentState == EnumMunchState.Kill) return;

        // Slowly starve the monster over time
        this.currentSatiety -= Time.deltaTime * 0.4f;
        this.currentSatiety = Mathf.Clamp(this.currentSatiety, 0, this.maxSatiety);

        this.UpdateMunchState();
        this.UpdateDebugText();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Ignore incoming food if we are already in the middle of killing the player
        if (this.currentState == EnumMunchState.Kill) return;

        Rigidbody foodRb = other.attachedRigidbody;
        if (foodRb == null) return;

        GameObject rootFoodObject = foodRb.gameObject;

        // The monster will only eat if it's hungry AND the object is actually tagged as food
        if (!rootFoodObject.CompareTag("Food") || this.currentState == EnumMunchState.NotHungry)
        {
            this.RejectItem(foodRb);
            return;
        }

        // Punish the player for throwing the food too aggressively
        if (this.IsMovingTooFast(foodRb))
        {
            this.RejectItem(foodRb);
        }
        else
        {
            this.ConsumeFood(rootFoodObject);
        }
    }

    #region Getters/Setters
    public float GetMaxAcceptableVelocity() => this.maxAcceptableVelocity;
    public void SetMaxAcceptableVelocity(float velocity) => this.maxAcceptableVelocity = velocity;
    public EnumMunchState GetCurrentState() => this.currentState;
    public BaseNavAIMonster.MonsterTypeEnum GetMonsterType() => this.monsterType;
    #endregion

    private void UpdateDebugText()
    {
        if (this.debugText != null)
        {
            this.debugText.text = $"State: {this.currentState}\nSatiety: {this.currentSatiety:F1}";
        }
    }

    private void UpdateMunchState()
    {
        // Continuously check if the dropping satiety value requires a state transition
        EnumMunchState newState = this.DetermineState(this.currentSatiety);

        if (newState != this.currentState)
        {
            this.ChangeState(newState);
        }
    }

    private EnumMunchState DetermineState(float satietyValue)
    {
        // Translate the numerical hunger value into actionable monster phases
        if (satietyValue > 40f) return EnumMunchState.NotHungry;
        if (satietyValue > 20f) return EnumMunchState.Hungry;
        if (satietyValue > 0f) return EnumMunchState.Angry;
        return EnumMunchState.Kill;
    }

    private IEnumerator MunchAndRelocate()
    {
        // Play the eating animation, wait for it to finish entirely, then move the monster to a new spawn point
        if (this.audioSource != null) this.audioSource.Stop();
        if (this.monsterAnimator != null)
            this.monsterAnimator.SetTrigger(this.munchTriggerName);

        yield return null;

        float animLength = this.monsterAnimator.GetCurrentAnimatorStateInfo(0).length;
        yield return new WaitForSeconds(animLength);
        Refactored.MonsterSpawner.Instance.RelocateMonster(this.transform.root, this.monsterType);
    }

    private void ChangeState(EnumMunchState newState)
    {
        this.currentState = newState;

        // Handle the audio and animation consequences of entering a new hunger phase
        switch (this.currentState)
        {
            case EnumMunchState.NotHungry:
                StartCoroutine(MunchAndRelocate());
                break;

            case EnumMunchState.Hungry:
                if (this.audioSource != null && this.hungrySound != null)
                {
                    this.audioSource.Stop(); // Cleanly stop the angry warning if fed back to Hungry
                    this.audioSource.clip = this.hungrySound;
                    this.audioSource.loop = true;
                    this.audioSource.volume = 1.0f;
                    this.audioSource.Play();
                }

                if (this.monsterAnimator != null)
                    this.monsterAnimator.SetTrigger(this.returnTriggerName);
                break;

            case EnumMunchState.Angry:
                if (this.audioSource != null && this.angryWarningSound != null)
                {
                    this.audioSource.Stop(); // Cleanly stop the hungry grumble
                    this.audioSource.clip = this.angryWarningSound;
                    this.audioSource.loop = true;
                    this.audioSource.volume = 1.0f;
                    this.audioSource.Play();
                }
                break;

            case EnumMunchState.Kill:
                if (this.audioSource != null)
                {
                    this.audioSource.Stop();
                    if (this.killJumpscareSound != null)
                    {
                        SoundEffectManager.Instance.PlaySoundFXClip(this.killJumpscareSound, this.transform, 0.5f);
                    }
                }

                DeathSystem.KillPlayer(DeathSystem.DeathEvent.DeathReason.Monster, false);
                break;
        }
    }

    private bool IsMovingTooFast(Rigidbody rb)
    {
        return rb.linearVelocity.magnitude > this.maxAcceptableVelocity;
    }

    private void ConsumeFood(GameObject foodObject)
    {
        // Ensure the player isn't still holding the item when it gets destroyed
        this.ForceRelease(foodObject.GetComponent<XRGrabInteractable>());

        // Feed the monster, cap its fullness, and recalculate its current mood
        this.currentSatiety += this.flatSatietyGain;
        this.currentSatiety = Mathf.Clamp(this.currentSatiety, 0, this.maxSatiety);
        this.UpdateMunchState();

        if (this.eatSound != null)
        {
            SoundEffectManager.Instance.PlaySoundFXClip(this.eatSound, this.transform, 0.5f);
        }

        Destroy(foodObject);
    }

    private void RejectItem(Rigidbody rb)
    {
        // Play the rejection animation and forcefully slap the item back toward the player
        if (this.monsterAnimator != null)
        {
            this.monsterAnimator.SetTrigger(this.rejectTriggerName);
        }

        XRGrabInteractable grabInteractable = rb.GetComponent<XRGrabInteractable>();
        this.ForceRelease(grabInteractable);

        Vector3 worldThrowDir = this.transform.TransformDirection(this.ThrowDirection);
        rb.AddForce(worldThrowDir * this.throwForce, ForceMode.Impulse);

        Debug.Log($"Monster slapped {rb.name} out of player's hand!");
    }

    private void ForceRelease(XRGrabInteractable interactable)
    {
        // Safely force the XR interaction manager to drop the grabbed item
        if (interactable != null && interactable.isSelected)
        {
            IXRInteractable iInteractable = interactable;
            interactable.interactionManager.SelectExit(interactable.firstInteractorSelecting, interactable);
        }
    }
}