using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Configuration for a single image within a screen fade.
/// Defines sprite, tint color, and rotation.
/// </summary>
[Serializable]
public struct ImageConfig
{
    [Tooltip("The sprite to display. Null = solid color rectangle.")]
    public Sprite sprite;

    [Tooltip("Tint color for the image. White = no tint (use sprite's original colors).")]
    public Color color;

    [Tooltip("Rotation of the image in euler angles.")]
    public int imageIndex;

    /// <summary>
    /// Creates a new image configuration.
    /// </summary>
    /// <param name="sprite">The sprite to display (null for solid color).</param>
    /// <param name="color">Tint color (white = no tint).</param>
    /// <param name="rotation">Rotation in euler angles.</param>
    public ImageConfig(Sprite sprite, Color color, int imageIndex)
    {
        this.sprite = sprite;
        this.color = color;
        this.imageIndex = imageIndex;
    }

    /// <summary>Creates a solid color config with no sprite.</summary>
    public static ImageConfig SolidColor(Color color) =>
        new ImageConfig(null, color, 0);

    /// <summary>Creates a config with a sprite and no tint (white).</summary>
    public static ImageConfig WithSprite(Sprite sprite, int imageIndex = 0) =>
        new ImageConfig(sprite, Color.white, imageIndex);
}

/// <summary>
/// Configuration for a screen fade animation.
/// </summary>
[Serializable]
public struct FadeConfig
{
    [Tooltip("Target opacity to fade to (0 = transparent, 1 = fully opaque).")]
    [Range(0f, 1f)]
    public float targetOpacity;

    [Tooltip("Duration of the fade animation in seconds.")]
    public float duration;

    [Tooltip("Image configurations. Null/empty = use existing setup unchanged.")]
    public ImageConfig[] imageConfigs;

    /// <summary>
    /// Creates a new fade configuration.
    /// </summary>
    /// <param name="targetOpacity">Target opacity (0-1).</param>
    /// <param name="duration">Fade duration in seconds.</param>
    /// <param name="deactivateOnComplete">Whether to deactivate canvas when fading to 0.</param>
    /// <param name="images">Optional image configurations.</param>
    public FadeConfig(float targetOpacity, float duration, ImageConfig[] imageConfigs = null)
    {
        this.targetOpacity = Mathf.Clamp01(targetOpacity);
        this.duration = duration;
        this.imageConfigs = imageConfigs;
    }

    /// <summary>Creates a simple fade to black.</summary>
    public static FadeConfig FadeToBlack(float duration = 2f) =>
        new FadeConfig(1f, duration, new[] { ImageConfig.SolidColor(Color.black) });

    /// <summary>Creates a simple fade to white.</summary>
    public static FadeConfig FadeToWhite(float duration = 2f) =>
        new FadeConfig(1f, duration, new[] { ImageConfig.SolidColor(Color.white) });

    /// <summary>Creates a fade to transparent (no image changes).</summary>
    public static FadeConfig FadeOut(float duration = 1f) =>
        new FadeConfig(0f, duration, null);
}

/// <summary>
/// Reusable screen fade component that animates a CanvasGroup's opacity.
/// Supports multiple images with different sprites, colors, and rotations.
/// Images are created dynamically as needed based on FadeConfig.
/// </summary>
public class ScreenFade : MonoBehaviour
{
    [Header("=== References ===")]
    [Tooltip("The CanvasGroup controlling the fade overlay opacity.")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    
    [SerializeField] private Image[] images;

    [Header("=== Default Configuration ===")]
    [Tooltip("Default fade configuration used when StartFade() is called without parameters.")]
    [SerializeField] private FadeConfig defaultConfig = new FadeConfig(1f, 2f, null);

    [Header("=== Performance ===")]
    [Tooltip("Seconds between fade updates. Higher = better performance, lower = smoother fade.")]
    [SerializeField] private float tickInterval = 0.05f;

    /// <summary>Fired when the fade animation completes.</summary>
    public event Action OnFadeComplete;

    private Timer fadeTimer;
    private float startOpacity;
    private FadeConfig activeConfig;

    /// <summary>Returns true if a fade is currently in progress.</summary>
    public bool IsFading => this.fadeTimer != null && this.fadeTimer.IsRunning;

    /// <summary>Current opacity of the fade overlay.</summary>
    public float CurrentOpacity => this.fadeCanvasGroup != null ? this.fadeCanvasGroup.alpha : 0f;

    private void Awake()
    {
        if (this.fadeCanvasGroup == null)
            this.fadeCanvasGroup = GetComponent<CanvasGroup>();

        if (this.images == null)
            this.images = GetComponentsInChildren<Image>();
    }

    private void Start()
    {
        if (this.fadeCanvasGroup != null)
        {
            this.fadeCanvasGroup.alpha = 0f;
            this.fadeCanvasGroup.blocksRaycasts = false;
        }
    }

    private void OnDestroy()
    {
        CleanupTimer();
    }

    /// <summary>
    /// Starts a fade using the default configuration.
    /// </summary>
    public void StartFade()
    {
        StartFadeWithConfig(this.defaultConfig);
    }

    /// <summary>
    /// Starts a fade with the specified configuration.
    /// </summary>
    /// <param name="config">The fade configuration to use.</param>
    public void StartFadeWithConfig(FadeConfig config)
    {
        if (this.fadeCanvasGroup == null) return;

        CleanupTimer();

        this.activeConfig = config;
        this.startOpacity = this.fadeCanvasGroup.alpha;

        // Configure images if provided
        if (config.imageConfigs != null && config.imageConfigs.Length > 0)
            ConfigureImages(config.imageConfigs);

        this.fadeTimer = new Timer(this.tickInterval, config.duration);
        this.fadeTimer.OnTimerTick += UpdateFade;
        this.fadeTimer.OnTimerFinished += CompleteFade;
        this.fadeTimer.Start();
    }

    /// <summary>
    /// Configures images based on the provided image configurations.
    /// Creates new images if needed, hides excess images.
    /// </summary>
    /// <param name="imageConfigs">Array of image configurations.</param>
    private void ConfigureImages(ImageConfig[] imageConfigs)
    {
        // Hide all images
        for (int i = images.Length; i > this.images.Length; i--)
        {
            this.images[i].enabled = false;
        }
        
        // Configure each image
        foreach (var imageConfig in imageConfigs)
        {
            var image = this.images[imageConfig.imageIndex];
            if (image != null)
            {
                image.enabled = true;
                image.sprite = imageConfig.sprite;
                image.color = imageConfig.color;
            }
        }
    }

    /// <summary>
    /// Immediately sets the opacity without animation.
    /// </summary>
    /// <param name="opacity">Target opacity (0-1).</param>
    public void SetOpacityImmediate(float opacity)
    {
        if (this.fadeCanvasGroup == null) return;

        CleanupTimer();

        opacity = Mathf.Clamp01(opacity);
        this.fadeCanvasGroup.alpha = opacity;
    }

    /// <summary>
    /// Stops the current fade and optionally resets to transparent.
    /// </summary>
    /// <param name="resetToTransparent">If true, immediately sets opacity to 0.</param>
    public void StopFade(bool resetToTransparent = false)
    {
        CleanupTimer();

        if (resetToTransparent)
            SetOpacityImmediate(0f);
    }

    /// <summary>
    /// Updates the fade opacity each tick based on elapsed time.
    /// </summary>
    private void UpdateFade()
    {
        if (this.fadeCanvasGroup == null || this.fadeTimer == null) return;

        float progress = Mathf.Clamp01(this.fadeTimer.Elapsed / this.activeConfig.duration);
        this.fadeCanvasGroup.alpha = Mathf.Lerp(this.startOpacity, this.activeConfig.targetOpacity, progress);
    }

    /// <summary>
    /// Called when the fade animation completes.
    /// Ensures final opacity is exact and fires the completion event.
    /// </summary>
    private void CompleteFade()
    {
        if (this.fadeCanvasGroup != null)
        {
            this.fadeCanvasGroup.alpha = this.activeConfig.targetOpacity;
        }

        CleanupTimer();
        OnFadeComplete?.Invoke();
    }

    /// <summary>
    /// Disposes the timer and removes event subscriptions.
    /// </summary>
    private void CleanupTimer()
    {
        if (this.fadeTimer == null) return;

        this.fadeTimer.OnTimerTick -= UpdateFade;
        this.fadeTimer.OnTimerFinished -= CompleteFade;
        this.fadeTimer.Dispose();
        this.fadeTimer = null;
    }
}
