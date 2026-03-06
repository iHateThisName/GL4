using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUIControler : MonoBehaviour
{
    [Header("=== References ===")]
    [SerializeField] private Button continueButton;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private TextMeshProUGUI resetTimerText;
    [SerializeField] private float sceneDuration = 5f;
    
    private Timer reloadSceneTimer;

    private void Start() {
        this.continueButton.onClick.AddListener(OnContinueClicked);
        
        UpdateDeathUI(DeathSystem.deathEvent);
        
        this.reloadSceneTimer = new Timer(0.1f, this.sceneDuration);
        this.reloadSceneTimer.OnTimerTick += OnResetTimerTicked;
        this.reloadSceneTimer.OnTimerFinished += ReloadGameScene;
        this.reloadSceneTimer.Start();
    }
    
    /// <summary>
    /// Clean up the timer when this component is destroyed.
    /// </summary>
    private void OnDestroy() {
        this.continueButton.onClick.RemoveListener(OnContinueClicked);
        
        if (this.reloadSceneTimer != null)
        {
            this.reloadSceneTimer.OnTimerTick -= OnResetTimerTicked;
            this.reloadSceneTimer.OnTimerFinished -= ReloadGameScene;
            this.reloadSceneTimer.Dispose();
            this.reloadSceneTimer = null;
        }
    }
    
    void OnContinueClicked() {
        GameManager.Instance.ContinueGame();
    }
    
    public void ReloadGameScene()
    {
        this.reloadSceneTimer.Dispose();
        GameManager.Instance.ContinueGame();
    }
    
    private void OnResetTimerTicked()
    {
        if (this.resetTimerText == null) return;

        var timeRemaining = Mathf.CeilToInt(this.reloadSceneTimer.Duration - this.reloadSceneTimer.Elapsed);
        
        timeRemaining = Mathf.Max(0, timeRemaining);
        
        float minutesRemaining = timeRemaining / 60;
        float secondsRemaining = timeRemaining % 60;
        
        this.resetTimerText.text = $"Time until reset: {minutesRemaining:00}:{secondsRemaining:00}";
    }
    
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
