using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controls the Game Over screen UI, displaying death reason and auto-restart countdown.
/// Placed in the GameOver scene and reads death information from DeathSystem.deathEvent.
/// </summary>
public class GameOverUIControler : MonoBehaviour
{
    [Header("=== UI References ===")]
    [SerializeField] private Button continueButton; // Button to manually restart the game
    [SerializeField] private TextMeshProUGUI gameOverText; // Displays death reason message
    [SerializeField] private TextMeshProUGUI resetTimerText; // Displays countdown until auto-restart

    [Header("=== Settings ===")]
    [SerializeField] private float sceneDuration = 5f; // Seconds before auto-restart

    private Timer reloadSceneTimer;

    /// <summary>
    /// Initializes the UI with death information and starts the auto-restart countdown.
    /// </summary>
    private void Start()
    {
        this.continueButton.onClick.AddListener(OnContinueClicked);

        // Display appropriate message based on death reason
        UpdateDeathUI(DeathSystem.deathEvent);

        // Start countdown timer for auto-restart
        this.reloadSceneTimer = new Timer(0.1f, this.sceneDuration);
        this.reloadSceneTimer.OnTimerTick += OnResetTimerTicked;
        this.reloadSceneTimer.OnTimerFinished += ReloadGameScene;
        this.reloadSceneTimer.Start();
    }

    /// <summary>
    /// Cleans up button listeners and disposes the timer to prevent memory leaks.
    /// </summary>
    private void OnDestroy()
    {
        this.continueButton.onClick.RemoveListener(OnContinueClicked);

        if (this.reloadSceneTimer != null)
        {
            this.reloadSceneTimer.OnTimerTick -= OnResetTimerTicked;
            this.reloadSceneTimer.OnTimerFinished -= ReloadGameScene;
            this.reloadSceneTimer.Dispose();
            this.reloadSceneTimer = null;
        }
    }

    /// <summary>
    /// Called when the Continue button is clicked. Immediately restarts the game.
    /// </summary>
    private void OnContinueClicked()
    {
        GameManager.Instance.ContinueGame();
    }

    /// <summary>
    /// Called when the countdown timer expires. Disposes timer and restarts the game.
    /// </summary>
    public void ReloadGameScene()
    {
        this.reloadSceneTimer.Dispose();
        GameManager.Instance.ContinueGame();
    }

    /// <summary>
    /// Updates the countdown display each timer tick.
    /// Shows remaining time in MM:SS format.
    /// </summary>
    private void OnResetTimerTicked()
    {
        if (this.resetTimerText == null) return;

        // Calculate remaining seconds
        var timeRemaining = Mathf.CeilToInt(this.reloadSceneTimer.Duration - this.reloadSceneTimer.Elapsed);
        timeRemaining = Mathf.Max(0, timeRemaining);

        // Convert to minutes and seconds for display
        float minutesRemaining = timeRemaining / 60;
        float secondsRemaining = timeRemaining % 60;

        this.resetTimerText.text = $"Time until reset: {minutesRemaining:00}:{secondsRemaining:00}";
    }

    /// <summary>
    /// Updates the game over text based on the death reason.
    /// Shows different messages for each death type and survival.
    /// </summary>
    /// <param name="deathEvent">The death event containing the reason.</param>
    private void UpdateDeathUI(DeathSystem.DeathEvent deathEvent)
    {
        if (this.gameOverText == null) return;

        string message = deathEvent.Reason switch
        {
            DeathSystem.DeathEvent.DeathReason.Temperature => "You froze to death!",
            DeathSystem.DeathEvent.DeathReason.Hunger => "You starved!",
            DeathSystem.DeathEvent.DeathReason.Monster => "The monster got you!",
            DeathSystem.DeathEvent.DeathReason.Survived => "Night Survived!",
            _ => "Game Over"
        };

        this.gameOverText.text = message;
        Debug.Log($"GameOverUI: Death reason - {deathEvent.Reason}");
    }
}
