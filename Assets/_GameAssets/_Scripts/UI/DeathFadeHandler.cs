using UnityEngine;

/// <summary>
/// Handles death-specific fade behavior. Builds a FadeConfig with the appropriate
/// death/survival color and delegates the full transition to SceneTransition.
/// </summary>
public class DeathFadeHandler : MonoBehaviour
{
    [Header("=== References ===")]
    [SerializeField] private SO_ScreenFadeRef screenFadeRef;

    [Header("=== Death Settings ===")]
    [SerializeField] private Color deathColor = Color.black;
    [SerializeField] private Color survivalColor = Color.white;
    [SerializeField] private float fadeDuration = 2f;

    private readonly ImageConfig[] singleImageBuffer = new ImageConfig[1];

    private void Start() => DeathSystem.OnPlayerDied += OnPlayerDeath;
    private void OnDisable() => DeathSystem.OnPlayerDied -= OnPlayerDeath;

    private void OnPlayerDeath()
    {
        Color fadeColor = DeathSystem.deathEvent.Reason == DeathSystem.DeathEvent.DeathReason.Survived
            ? this.survivalColor
            : this.deathColor;

        this.singleImageBuffer[0] = ImageConfig.SolidColor(fadeColor);
        var fadeOutConfig = new FadeConfig(1f, this.fadeDuration, this.singleImageBuffer);

        SceneTransition.LoadScene("GameOver", fadeOutConfig, this.screenFadeRef);
    }
}
