using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Handles the screen fade effect when the player dies.
/// Listens to DeathSystem events and fades the screen before transitioning to the GameOver scene.
/// </summary>
/// <remarks>
/// The fade color changes based on death reason: black for death, white for survival.
/// After the fade completes, triggers the scene transition via DeathSystem.
/// </remarks>
public class DeathFade : MonoBehaviour
{
    [Header("=== References ===")]
    [Tooltip("The CanvasGroup controlling the fade overlay opacity.")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private SO_ScreenFadeRef screenFadeRef;

    [Header("=== Fade Settings ===")]
    [Tooltip("The color used for the fade overlay.")]
    [SerializeField] private Color fadeColor = Color.black;

    [Tooltip("Duration of the fade-to-black animation in seconds.")]
    [SerializeField] private float fadeDuration = 2.0f;

    [Tooltip("Duration to hold at full opacity before transitioning.")]
    [SerializeField] private float fadeWaitDuration = 0.5f;

    private TimerHandle fadeTimerHandle;
    private Image fadeImage;

    /// <summary>Fired when the fade animation completes, before scene transition.</summary>
    public System.Action OnFadeFinished;

    /// <summary>
    /// Initializes the fade canvas to be invisible and inactive.
    /// </summary>
    private void Start()
    {
        // Try to get CanvasGroup from this object if not assigned
        if (this.fadeCanvasGroup == null)
            this.fadeCanvasGroup = GetComponent<CanvasGroup>();

        if (this.fadeCanvasGroup != null)
        {
            this.fadeCanvasGroup.alpha = 0f;
            this.fadeCanvasGroup.blocksRaycasts = false;

            // Disable canvas until needed to save rendering cost
            this.fadeCanvasGroup.gameObject.SetActive(false);
        }

        // Cache the Image component for color changes
        this.fadeImage = this.fadeCanvasGroup.gameObject.GetComponentInChildren<Image>();
        if (this.fadeImage != null)
            this.fadeImage.color = this.fadeColor;
    }

    /// <summary>
    /// Subscribes to DeathSystem events when enabled.
    /// </summary>
    private void OnEnable()
    {
        DeathSystem.OnPlayerDied += OnPlayerDeath;
    }

    /// <summary>
    /// Unsubscribes from DeathSystem events when disabled.
    /// </summary>
    private void OnDisable()
    {
        DeathSystem.OnPlayerDied -= OnPlayerDeath;
    }

    /// <summary>
    /// Cleans up the fade timer to prevent memory leaks.
    /// </summary>
    private void OnDestroy()
    {
        TimerManager.Release(ref this.fadeTimerHandle);
    }

    /// <summary>
    /// Called when the player dies. Starts the fade animation.
    /// Uses white fade for survival, default color for death.
    /// </summary>
    public void OnPlayerDeath()
    {
        if (this.fadeCanvasGroup == null) return;

        // Activate and show the fade canvas
        this.fadeCanvasGroup.gameObject.SetActive(true);

        TimerManager.Release(ref this.fadeTimerHandle);
        this.fadeTimerHandle = TimerManager.Create(0.1f, this.fadeDuration + this.fadeWaitDuration);
        TimerManager.SetCallbacks(this.fadeTimerHandle, TickFade, FinishFade);

        // Use white fade for survival (win condition)
        if (DeathSystem.deathEvent.Reason == DeathSystem.DeathEvent.DeathReason.Survived)
            this.fadeImage.color = Color.white;
    }

    /// <summary>
    /// Updates the fade opacity each tick based on elapsed time.
    /// </summary>
    private void TickFade()
    {
        if (this.fadeCanvasGroup == null || !this.fadeCanvasGroup.gameObject.activeInHierarchy) return;

        float elapsed = TimerManager.Validate(this.fadeTimerHandle) ? TimerManager.GetRef(this.fadeTimerHandle).Elapsed : 0f;
        this.fadeCanvasGroup.alpha = Mathf.Clamp01(elapsed / this.fadeDuration);
    }

    /// <summary>
    /// Called when the fade animation completes.
    /// Triggers the OnFadeFinished event and initiates scene transition.
    /// </summary>
    private void FinishFade()
    {
        this.OnFadeFinished?.Invoke();
        DeathSystem.deathEvent.LoadScene(this.screenFadeRef);
    }
}
