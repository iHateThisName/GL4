using UnityEngine;

/// <summary>
/// Handles death-specific fade behavior by listening to DeathSystem events.
/// Uses a ScreenFade component for the actual fade animation.
/// </summary>
/// <remarks>
/// Attach this alongside or reference a ScreenFade component.
/// Automatically uses different fade configurations based on death reason.
/// </remarks>
public class DeathFadeHandler : MonoBehaviour
{
    [Header("=== References ===")]
    [Tooltip("The ScreenFade component to use for the fade animation.")]
    [SerializeField] private ScreenFade screenFade;

    [Header("=== Death Settings ===")]
    [Tooltip("Color used when player dies (temperature, hunger, monster).")]
    [SerializeField] private Color deathColor = Color.black;

    [Tooltip("Color used when player survives the night.")]
    [SerializeField] private Color survivalColor = Color.white;

    [Tooltip("Duration of the fade animation in seconds.")]
    [SerializeField] private float fadeDuration = 2f;

    [Header("=== Timing ===")]
    [Tooltip("Additional wait time at full opacity before loading scene.")]
    [SerializeField] private float waitAfterFade = 0.5f;

    private Timer waitTimer;

    private void Awake()
    {
        if (this.screenFade == null)
            this.screenFade = GetComponent<ScreenFade>();
    }

    private void OnEnable()
    {
        DeathSystem.OnPlayerDied += OnPlayerDeath;
    }

    private void OnDisable()
    {
        DeathSystem.OnPlayerDied -= OnPlayerDeath;
    }

    private void OnDestroy()
    {
        CleanupWaitTimer();
    }

    /// <summary>
    /// Called when the player dies. Starts the appropriate fade animation.
    /// </summary>
    private void OnPlayerDeath()
    {
        if (this.screenFade == null) return;

        // Select color based on death reason
        Color fadeColor = DeathSystem.deathEvent.Reason == DeathSystem.DeathEvent.DeathReason.Survived
            ? this.survivalColor
            : this.deathColor;

        // Create config with solid color image
        FadeConfig config = new FadeConfig(
            targetOpacity: 1f,
            duration: this.fadeDuration,
            imageConfigs: new[] { ImageConfig.SolidColor(fadeColor) }
        );

        this.screenFade.OnFadeComplete += OnFadeComplete;
        this.screenFade.StartFadeWithConfig(config);
    }

    /// <summary>
    /// Called when the fade animation completes.
    /// Starts the wait timer before loading the scene.
    /// </summary>
    private void OnFadeComplete()
    {
        this.screenFade.OnFadeComplete -= OnFadeComplete;

        if (this.waitAfterFade > 0f)
        {
            this.waitTimer = new Timer(this.waitAfterFade, this.waitAfterFade);
            this.waitTimer.OnTimerFinished += LoadDeathScene;
            this.waitTimer.Start();
        }
        else
        {
            LoadDeathScene();
        }
    }

    /// <summary>
    /// Loads the death/game over scene via DeathSystem.
    /// </summary>
    private void LoadDeathScene()
    {
        CleanupWaitTimer();
        DeathSystem.deathEvent.LoadScene();
    }

    /// <summary>
    /// Disposes the wait timer and removes event subscriptions.
    /// </summary>
    private void CleanupWaitTimer()
    {
        if (this.waitTimer == null) return;

        this.waitTimer.OnTimerFinished -= LoadDeathScene;
        this.waitTimer.Dispose();
        this.waitTimer = null;
    }
}
