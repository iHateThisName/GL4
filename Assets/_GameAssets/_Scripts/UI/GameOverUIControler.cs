using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUIControler : MonoBehaviour
{

    [Header("=== References ===")] 
    [SerializeField] private GameOverManager controller;
    [SerializeField] private Button continueButton;
    [SerializeField] private TextMeshProUGUI gameOverText;
    [SerializeField] private TextMeshProUGUI resetTimerText;

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    private void Start() {
        this.continueButton.onClick.AddListener(OnContinueClicked);
        
        UpdateDeathUI(DeathSystem.deathEvent);
        if (controller != null)
            controller.Timer.OnTimerTick += OnResetTimerTicked;
    }

    private void OnDestroy() {
        this.continueButton.onClick.RemoveListener(OnContinueClicked);
        
        if (controller != null)
            controller.Timer.OnTimerTick -= OnResetTimerTicked;
    }
    void OnContinueClicked() {
        GameManager.Instance.ContinueGame();
    }
    
    private void OnResetTimerTicked()
    {
        if (this.resetTimerText == null) return;

        var timeRemaining = Mathf.CeilToInt(this.controller.Timer.Duration - this.controller.Timer.Elapsed);
        
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
