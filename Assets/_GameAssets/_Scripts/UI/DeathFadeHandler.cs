using UnityEngine;

/// <summary>
/// Handles death-specific fade behavior. Builds a FadeConfig with the appropriate
/// death/survival color and delegates the full transition to SceneTransition.
/// When the player survives the final night, loads the victory scene instead of the game-over scene.
/// </summary>
public class DeathFadeHandler : MonoBehaviour
{
    [Header("=== References ===")]
    [SerializeField] private SO_ScreenFadeRef screenFadeRef;
    [SerializeField] private SO_NightSettings nightSettings;

    [Header("=== Scene Indices ===")]
    [SerializeField] private int gameOverSceneIndex = 2;
    [SerializeField] private int victorySceneIndex = 3;

    [Header("=== Death Settings ===")]
    [SerializeField] private Color deathColor = Color.black;
    [SerializeField] private Color survivalColor = Color.white;
    [SerializeField] private float fadeDuration = 2f;

    private readonly ImageConfig[] singleImageBuffer = new ImageConfig[1];

    private void Start() => DeathSystem.OnPlayerDied += OnPlayerDeath;
    private void OnDisable() => DeathSystem.OnPlayerDied -= OnPlayerDeath;

    private void OnPlayerDeath()
    {
        bool survived = DeathSystem.deathEvent.Reason == DeathSystem.DeathEvent.DeathReason.Survived;

        Color fadeColor = survived ? this.survivalColor : this.deathColor;
        this.singleImageBuffer[0] = ImageConfig.SolidColor(fadeColor);
        var fadeOutConfig = new FadeConfig(1f, this.fadeDuration, this.singleImageBuffer);

        int targetScene = ResolveTargetScene(survived);
        SceneTransition.LoadScene(targetScene, fadeOutConfig, this.screenFadeRef);
    }

    private int ResolveTargetScene(bool survived)
    {
        if (survived && nightSettings != null)
        {
            int currentNight = GameManager.Instance != null ? GameManager.Instance.GetCurrentNight() : 1;
            if (currentNight >= nightSettings.GetFinalNight())
            {
                Debug.Log($"[DeathFadeHandler] Night {currentNight} is the final night. Loading victory scene.");
                return this.victorySceneIndex;
            }
        }
        return this.gameOverSceneIndex;
    }
}
