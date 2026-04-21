using System;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Configuration for a single image within a screen fade.
/// </summary>
[Serializable]
public struct ImageConfig
{
    [Tooltip("The sprite to display. Null = solid color rectangle.")]
    public Sprite sprite;

    [Tooltip("Tint color for the image. White = no tint (use sprite's original colors).")]
    public Color color;

    [Tooltip("Index into the images array.")]
    public int imageIndex;

    public ImageConfig(Sprite sprite, Color color, int imageIndex)
    {
        this.sprite = sprite;
        this.color = color;
        this.imageIndex = imageIndex;
    }

    public static ImageConfig SolidColor(Color color) =>
        new ImageConfig(null, color, 0);

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
    
    private const float DEFAULT_FADE_DURATION = 2f;

    public FadeConfig(float targetOpacity, float duration, ImageConfig[] imageConfigs = null)
    {
        this.targetOpacity = Mathf.Clamp01(targetOpacity);
        this.duration = duration;
        this.imageConfigs = imageConfigs;
    }

    // Cached static arrays — no allocation per call
    private static readonly ImageConfig[] BlackImage = { ImageConfig.SolidColor(Color.black) };
    private static readonly ImageConfig[] WhiteImage = { ImageConfig.SolidColor(Color.white) };

    public static FadeConfig FadeToBlack(float duration = DEFAULT_FADE_DURATION) =>
        new FadeConfig(1f, duration, BlackImage);

    public static FadeConfig FadeToWhite(float duration = DEFAULT_FADE_DURATION) =>
        new FadeConfig(1f, duration, WhiteImage);

    public static FadeConfig FadeOut(float duration = DEFAULT_FADE_DURATION) =>
        new FadeConfig(0f, duration, null);
}

/// <summary>
/// Reusable screen fade component using async Awaitable. Zero coroutine/timer allocations.
/// </summary>
public class ScreenFade : MonoBehaviour
{
    [Header("=== References ===")]
    [SerializeField] private SO_ScreenFadeRef screenFadeRef;
    [SerializeField] private CanvasGroup fadeCanvasGroup;
    [SerializeField] private Image[] images = new Image[0];

    [Header("=== Default Configuration ===")]
    [SerializeField] private FadeConfig defaultConfig = new FadeConfig(1f, 2f, null);

    /// <summary>Fired when the fade animation completes. Kept for backward compatibility.</summary>
    public event Action OnFadeComplete;

    private CancellationTokenSource fadeCts;

    public bool IsFading { get; private set; }
    public float CurrentOpacity => this.fadeCanvasGroup != null ? this.fadeCanvasGroup.alpha : 0f;

    private void Awake()
    {
        if (this.fadeCanvasGroup == null)
            this.fadeCanvasGroup = GetComponent<CanvasGroup>();

        if (this.images == null)
            this.images = GetComponentsInChildren<Image>();

        if (this.screenFadeRef != null)
            this.screenFadeRef.Value = this;
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
        CancelFade();
        if (this.screenFadeRef != null && this.screenFadeRef.Value == this)
            this.screenFadeRef.Value = null;
    }

    /// <summary>
    /// Starts a fade using the default configuration. Fire-and-forget.
    /// </summary>
    public void StartFade() => StartFadeWithConfig(this.defaultConfig);

    /// <summary>
    /// Starts a fade with the specified configuration. Fire-and-forget.
    /// </summary>
    public void StartFadeWithConfig(FadeConfig config)
    {
        if (this.fadeCanvasGroup == null) return;

        CancelFade();

        if (config.imageConfigs != null && config.imageConfigs.Length > 0)
            ConfigureImages(config.imageConfigs);

        this.fadeCts = new CancellationTokenSource();
        _ = RunFadeAsync(config, this.fadeCts.Token);
    }

    /// <summary>
    /// Awaitable version — callers can directly await completion. Zero allocation.
    /// </summary>
    public async Awaitable FadeAsync(FadeConfig config, CancellationToken ct = default)
    {
        if (this.fadeCanvasGroup == null)
        {
            Debug.LogWarning($"[ScreenFade] FadeAsync aborted — fadeCanvasGroup is null on '{gameObject.name}'");
            return;
        }

        Debug.Log($"[ScreenFade] FadeAsync: {this.fadeCanvasGroup.alpha} → {config.targetOpacity} over {config.duration}s on '{gameObject.name}'");

        CancelFade();

        if (config.imageConfigs != null && config.imageConfigs.Length > 0)
            ConfigureImages(config.imageConfigs);

        await RunFadeAsync(config, ct);
    }

    private async Awaitable RunFadeAsync(FadeConfig config, CancellationToken ct)
    {
        IsFading = true;
        float startOpacity = this.fadeCanvasGroup.alpha;
        float elapsed = 0f;

        // DIAGNOSTIC: log first frame to verify the loop is running
        bool loggedFirstFrame = false;

        while (elapsed < config.duration)
        {
            if (ct.IsCancellationRequested) break;

            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / config.duration);
            this.fadeCanvasGroup.alpha = Mathf.Lerp(startOpacity, config.targetOpacity, t);

            if (!loggedFirstFrame)
            {
                Debug.Log($"[ScreenFade] RunFadeAsync first frame: deltaTime={Time.deltaTime}, unscaledDeltaTime={Time.unscaledDeltaTime}, alpha={this.fadeCanvasGroup.alpha}, elapsed={elapsed}, canvasGroup.alpha actually={this.fadeCanvasGroup.alpha}, gameObject.activeInHierarchy={gameObject.activeInHierarchy}");
                loggedFirstFrame = true;
            }

            await Awaitable.NextFrameAsync(ct);
        }

        if (!ct.IsCancellationRequested)
        {
            this.fadeCanvasGroup.alpha = config.targetOpacity;
            IsFading = false;
            OnFadeComplete?.Invoke();
        }
    }

    public void SetOpacityImmediate(float opacity)
    {
        if (this.fadeCanvasGroup == null) return;
        CancelFade();
        this.fadeCanvasGroup.alpha = Mathf.Clamp01(opacity);
    }

    public void StopFade(bool resetToTransparent = false)
    {
        CancelFade();
        if (resetToTransparent) SetOpacityImmediate(0f);
    }

    private void CancelFade()
    {
        if (this.fadeCts != null)
        {
            this.fadeCts.Cancel();
            this.fadeCts.Dispose();
            this.fadeCts = null;
        }
        IsFading = false;
    }

    private void ConfigureImages(ImageConfig[] imageConfigs)
    {
        foreach (var imageConfig in imageConfigs)
        {
            if (this.images == null || imageConfig.imageIndex >= this.images.Length)
            {
                Debug.LogWarning($"[ScreenFade] ConfigureImages: imageIndex {imageConfig.imageIndex} out of range (images.Length={this.images?.Length ?? 0}) on '{gameObject.name}'");
                continue;
            }
            var image = this.images[imageConfig.imageIndex];
            if (image != null)
            {
                image.enabled = true;
                image.sprite = imageConfig.sprite;
                image.color = imageConfig.color;
            }
        }
    }
}
