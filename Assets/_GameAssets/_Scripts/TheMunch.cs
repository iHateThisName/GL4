using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using TMPro;

public enum MunchState
{
    NotHungry,
    Hungry,
    Angry,
    Kill
}

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

    public Vector3 ThrowDirection = new Vector3(0, 1, 1);

    [Header("Debug UI")]
    [Tooltip("Assign a TextMeshPro (UI or 3D) component to see live updates.")]
    [SerializeField] private TMP_Text debugText;

    private MunchState currentState;

    private void Start()
    {
        this.currentSatiety = this.maxSatiety;

        // Force the initial state to run so the arm retracts immediately on load
        this.ChangeState(MunchState.NotHungry);
    }

    private void Update()
    {
        if (this.currentState == MunchState.Kill) return;

        this.currentSatiety -= Time.deltaTime * 0.2f;
        this.currentSatiety = Mathf.Clamp(this.currentSatiety, 0, this.maxSatiety);

        this.UpdateMunchState();
        this.UpdateDebugText();
    }

    private void UpdateDebugText()
    {
        if (this.debugText != null)
        {
            this.debugText.text = $"State: {this.currentState}\nSatiety: {this.currentSatiety:F1}";
        }
    }

    private void UpdateMunchState()
    {
        MunchState newState = this.DetermineState(this.currentSatiety);

        if (newState != this.currentState)
        {
            this.ChangeState(newState);
        }
    }

    private MunchState DetermineState(float satietyValue)
    {
        if (satietyValue > 40f) return MunchState.NotHungry;
        if (satietyValue > 20f) return MunchState.Hungry;
        if (satietyValue > 0f) return MunchState.Angry;
        return MunchState.Kill;
    }

    private void ChangeState(MunchState newState)
    {
        this.currentState = newState;

        switch (this.currentState)
        {
            case MunchState.NotHungry:
                if (this.audioSource != null) this.audioSource.Stop(); // Stop everything

                if (this.monsterAnimator != null)
                    this.monsterAnimator.SetTrigger(this.munchTriggerName);
                break;

            case MunchState.Hungry:
                if (this.audioSource != null && this.hungrySound != null)
                {
                    this.audioSource.Stop(); // Cleanly stop the angry warning if fed back to Hungry
                    this.audioSource.clip = this.hungrySound;
                    this.audioSource.loop = true;
                    this.audioSource.volume = 1.0f; // Ensure volume is 100%
                    this.audioSource.Play();
                }

                if (this.monsterAnimator != null)
                    this.monsterAnimator.SetTrigger(this.returnTriggerName);
                break;

            case MunchState.Angry:
                if (this.audioSource != null && this.angryWarningSound != null)
                {
                    this.audioSource.Stop(); // Cleanly stop the hungry grumble
                    this.audioSource.clip = this.angryWarningSound;
                    this.audioSource.loop = true;
                    this.audioSource.volume = 1.0f; // Ensure volume is 100%
                    this.audioSource.Play();
                }
                break;

            case MunchState.Kill:
                if (this.audioSource != null)
                {
                    this.audioSource.Stop();
                    if (this.killJumpscareSound != null)
                    {
                        SoundEffectManager.Instance.PlaySoundFXClip(this.killJumpscareSound, transform, 0.5f);
                    }
                }
                
                DeathSystem.KillPlayer(DeathSystem.DeathEvent.DeathReason.Monster, false);
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (this.currentState == MunchState.Kill) return;

        Rigidbody parentRb = other.attachedRigidbody;
        if (parentRb == null) return;
        
        Transform foodObject = other.transform.parent;

        if (!foodObject.CompareTag("Food") || this.currentState == MunchState.NotHungry) 
        {
            this.RejectItem(parentRb);
            return;
        }
            
        if (this.IsMovingTooFast(parentRb))
        {
            this.RejectItem(parentRb);
        }
        else
        {
            this.ConsumeFood(foodObject.gameObject);
        }
    }

    private bool IsMovingTooFast(Rigidbody rb)
    {
        return rb.linearVelocity.magnitude > this.maxAcceptableVelocity;
    }

    private void ConsumeFood(GameObject foodObject)
    {
        this.ForceRelease(foodObject.GetComponent<XRGrabInteractable>());

        float valueToAdd = 20f;

        this.currentSatiety += valueToAdd;
        this.currentSatiety = Mathf.Clamp(this.currentSatiety, 0, this.maxSatiety);

        this.UpdateMunchState();
        if (eatSound == null) return;
        SoundEffectManager.Instance.PlaySoundFXClip(this.eatSound, transform, 1f);
        Destroy(foodObject, 2f);
    }

    private void RejectItem(Rigidbody rb)
    {
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
        if (interactable != null && interactable.isSelected)
        {
            IXRInteractable iInteractable = interactable;
            interactable.interactionManager.SelectExit(interactable.firstInteractorSelecting, interactable);
        }
    }

    #region Getters/Setters
    public float GetMaxAcceptableVelocity() => this.maxAcceptableVelocity;
    public void SetMaxAcceptableVelocity(float velocity) => this.maxAcceptableVelocity = velocity;
    public MunchState GetCurrentState() => this.currentState;
    #endregion
}