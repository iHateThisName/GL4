using TMPro;
using UnityEngine;

/// <summary>
/// Controls the Game Over screen UI, displaying death reason and auto-restart countdown.
/// Placed in the GameOver scene and reads death information from DeathSystem.deathEvent.
/// </summary>
public class GameOverUIControler : MonoBehaviour
{
    [Header("=== UI References ===")]
    [SerializeField] private TextMeshProUGUI gameOverText; // Displays death reason message
    [SerializeField] private TextMeshProUGUI bigGameOverText;

    /// <summary>
    /// Initializes the UI with death information and starts the auto-restart countdown.
    /// </summary>
    private void Start()
    {
        // Display appropriate message based on death reason
        UpdateDeathUI(DeathSystem.deathEvent);
    }

    /// <summary>
    /// Called when the Continue button is clicked. Immediately restarts the game.
    /// </summary>
    public void ContinueGame()
    {
        GameManager.Instance.ContinueGame();
    }

    //Setting for going back to the main menu
    public void BackToMenu()
    {
        GameManager.Instance.LoadScene("MainMenu");
        //SceneManager.LoadScene("MainMenu");
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
            DeathSystem.DeathEvent.DeathReason.Hunger => "You starved to death!",
            DeathSystem.DeathEvent.DeathReason.Monster => "The " + deathEvent.AdditionalInfo + " got you!",
            DeathSystem.DeathEvent.DeathReason.Survived => "You survived the Night!",
            _ => "Game Over"
        };

        this.gameOverText.text = message;

        if (deathEvent.Reason == DeathSystem.DeathEvent.DeathReason.Survived) {
            this.bigGameOverText.text = "Congratulations";
        } else {
            this.bigGameOverText.text = "Game Over";
        }
        Debug.Log($"GameOverUI: Death reason - {deathEvent.Reason}");
    }
}
