using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUIControler : MonoBehaviour {

    [Header("=== References ===")]
    [SerializeField] private Button continueButton;
    [SerializeField] private TextMeshProUGUI gameOverText;

    private void Start() {
        this.continueButton.onClick.AddListener(OnContinueClicked);
        
        UpdateDeathUI(DeathSystem.deathEvent);
    }

    private void OnDestroy() {
        this.continueButton.onClick.RemoveListener(OnContinueClicked);

    }
    void OnContinueClicked() {
        GameManager.Instance.ContinueGame();
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
