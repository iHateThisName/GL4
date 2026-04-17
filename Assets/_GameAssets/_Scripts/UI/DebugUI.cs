using System.Threading;
using TMPro;
using UnityEngine;
using static PlayerTemperatureSimulator;

/// <summary>
/// Manages the player's UI display.
/// </summary>
public class DebugUI : MonoBehaviour {
    [Header("Refrences")]
    [SerializeField] private TMP_Text temperatureText;
    [SerializeField] private TMP_Text locationText;
    [SerializeField] private TextMeshProUGUI hungerText;
    [SerializeField] private TextMeshProUGUI nightTimeText;
    [SerializeField] private TMP_Text heatModifer;

    [Header("Vision Effect Refrences")]
    [SerializeField] private CanvasGroup heatCanvasGroup;
    [SerializeField] private CanvasGroup coldCanvasGroup;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 5f;
    private const float DEFAULT_MEDIUM_EFFECT_ALPHA = 0.125f;
    private const float DEFAULT_HIGH_EFFECT_ALPHA = 0.25f;

    private bool useDebugInfo = false;
    private float uiUpdateTimer;
    private CancellationTokenSource warmFadeCts;
    private CancellationTokenSource coldFadeCts;

    private void Awake() {
        this.useDebugInfo = true;
    }

    private void Start() {
        if (!this.useDebugInfo) return;

        HandleTemperatureChanged(new BodyTemperatureStateChange {
            CurrentState = PlayerTemperatureSimulator.Instance.CurrentBodyTemperatureState
        });
        HandleHungerChanged(100);
        HandleLocationChanged(PlayerTemperatureSimulator.Instance.CurrentLocationType);
        HandleHeatModifierChanged(PlayerTemperatureSimulator.Instance.CurrentHeatModifier);
    }

    private void OnEnable() {
        this.temperatureText.text = "";
        this.hungerText.text = "";
        this.locationText.text = "";
        this.heatModifer.text = "";
        if (!this.useDebugInfo) return;

        PlayerTemperatureSimulator.OnBodyTemperatureStateChanged += HandleTemperatureChanged;
        PlayerTemperatureSimulator.OnLocationTypeChanged += HandleLocationChanged;
        HungerSystem.OnHungerChanged += HandleHungerChanged;
        PlayerTemperatureSimulator.OnHeatModifierChanged += HandleHeatModifierChanged;
    }

    private void OnDisable() {
        if (!this.useDebugInfo) return;

        PlayerTemperatureSimulator.OnBodyTemperatureStateChanged -= HandleTemperatureChanged;
        PlayerTemperatureSimulator.OnLocationTypeChanged -= HandleLocationChanged;
        HungerSystem.OnHungerChanged -= HandleHungerChanged;
        PlayerTemperatureSimulator.OnHeatModifierChanged -= HandleHeatModifierChanged;
    }

    private void OnDestroy() {
        CancelFade(ref this.warmFadeCts);
        CancelFade(ref this.coldFadeCts);
    }

    private void Update() {
        if (this.nightTimeText == null) return;
        this.uiUpdateTimer += Time.deltaTime;
        if (this.uiUpdateTimer < 1f) return;
        this.uiUpdateTimer = 0f;
        this.nightTimeText.text = "Time: " + GameManager.Instance.NightTime.ToString("F2");
    }

    private void HandleTemperatureChanged(BodyTemperatureStateChange change) {
        this.temperatureText.text = $"Temperature State: {change.CurrentState}";
        UpdateColor(change.CurrentState);

        if (change.CurrentState == EnumBodyTemperatureState.Normal && change.PreviousState == EnumBodyTemperatureState.MildHyperthermia)
            StartFadeCanvasGroup(canvasGroup: this.heatCanvasGroup, fadeIn: false);
        else if (change.CurrentState == EnumBodyTemperatureState.MildHyperthermia)
            StartFadeCanvasGroup(canvasGroup: this.heatCanvasGroup, fadeIn: true, targetAlpha: DEFAULT_MEDIUM_EFFECT_ALPHA);
        else if (change.CurrentState == EnumBodyTemperatureState.ModerateHypothermia)
            StartFadeCanvasGroup(canvasGroup: this.heatCanvasGroup, fadeIn: true, targetAlpha: DEFAULT_HIGH_EFFECT_ALPHA);

        if (change.CurrentState == EnumBodyTemperatureState.Normal && change.PreviousState == EnumBodyTemperatureState.MildHypothermia)
            StartFadeCanvasGroup(canvasGroup: this.coldCanvasGroup, fadeIn: false);
        else if (change.CurrentState == EnumBodyTemperatureState.MildHypothermia)
            StartFadeCanvasGroup(canvasGroup: this.coldCanvasGroup, fadeIn: true, targetAlpha: DEFAULT_MEDIUM_EFFECT_ALPHA);
        else if (change.CurrentState == EnumBodyTemperatureState.ModerateHypothermia)
            StartFadeCanvasGroup(canvasGroup: this.coldCanvasGroup, fadeIn: true, targetAlpha: DEFAULT_HIGH_EFFECT_ALPHA);
    }

    private void UpdateColor(EnumBodyTemperatureState state) {
        switch (state) {
            case EnumBodyTemperatureState.ModerateHypothermia:
                this.temperatureText.color = Color.blue;
                break;
            case EnumBodyTemperatureState.MildHypothermia:
                this.temperatureText.color = Color.cyan;
                break;
            case EnumBodyTemperatureState.Normal:
                this.temperatureText.color = Color.green;
                break;
            case EnumBodyTemperatureState.MildHyperthermia:
                this.temperatureText.color = Color.yellow;
                break;
            case EnumBodyTemperatureState.ModerateHyperthermia:
                this.temperatureText.color = Color.red;
                break;
            case EnumBodyTemperatureState.Hyperthermia:
            case EnumBodyTemperatureState.Hypothermia:
                this.temperatureText.color = Color.magenta;
                break;
        }
    }

    private void HandleHungerChanged(float hunger) {
        this.hungerText.text = "Hunger: " + hunger.ToString("F2");
    }

    private void HandleLocationChanged(EnumLocationType type) {
        this.locationText.text = $"Location: {PlayerTemperatureSimulator.Instance.CurrentLocationType}";
        EnumBodyTemperatureState currentBodyTemperatureState = PlayerTemperatureSimulator.Instance.CurrentBodyTemperatureState;

        if (type == EnumLocationType.Warm) {
            if (currentBodyTemperatureState == EnumBodyTemperatureState.ModerateHyperthermia)
                StartFadeCanvasGroup(this.heatCanvasGroup, fadeIn: true, targetAlpha: DEFAULT_HIGH_EFFECT_ALPHA);
            else
                StartFadeCanvasGroup(this.heatCanvasGroup, fadeIn: true, targetAlpha: DEFAULT_MEDIUM_EFFECT_ALPHA);
        } else if (currentBodyTemperatureState != EnumBodyTemperatureState.MildHyperthermia || currentBodyTemperatureState != EnumBodyTemperatureState.ModerateHyperthermia) {
            if (!TemperatureZoneManager.Instance.IsPlayerInZone(EnumLocationType.Warm))
                StartFadeCanvasGroup(this.heatCanvasGroup, fadeIn: false);
        }
    }

    private void StartFadeCanvasGroup(CanvasGroup canvasGroup, bool fadeIn, float targetAlpha = -1) {
        bool isWarmEffect = canvasGroup == this.heatCanvasGroup;

        if (isWarmEffect) {
            CancelFade(ref this.warmFadeCts);
            this.warmFadeCts = new CancellationTokenSource();
            _ = FadeCanvasGroupAsync(canvasGroup, fadeIn, targetAlpha, this.warmFadeCts.Token);
        } else {
            CancelFade(ref this.coldFadeCts);
            this.coldFadeCts = new CancellationTokenSource();
            _ = FadeCanvasGroupAsync(canvasGroup, fadeIn, targetAlpha, this.coldFadeCts.Token);
        }
    }

    private async Awaitable FadeCanvasGroupAsync(CanvasGroup canvasGroup, bool fadeIn, float targetAlpha, CancellationToken ct) {
        //Debug.Log($"Starting fade {(fadeIn ? "in" : "out")} for {canvasGroup.gameObject.name} to target alpha {targetAlpha}");

        if (canvasGroup == null) return;

        if (fadeIn && !canvasGroup.gameObject.activeInHierarchy)
            canvasGroup.gameObject.SetActive(true);

        float startAlpha = canvasGroup.alpha;
        if (targetAlpha == -1)
            targetAlpha = fadeIn ? DEFAULT_HIGH_EFFECT_ALPHA : 0f;

        float elapsedTime = 0f;

        while (elapsedTime < this.fadeDuration) {
            if (ct.IsCancellationRequested) return;

            elapsedTime += Time.deltaTime;
            float t = elapsedTime / this.fadeDuration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            await Awaitable.NextFrameAsync(ct);
        }

        canvasGroup.alpha = targetAlpha;

        if (!fadeIn)
            canvasGroup.gameObject.SetActive(false);
    }

    private static void CancelFade(ref CancellationTokenSource cts) {
        if (cts != null) {
            cts.Cancel();
            cts.Dispose();
            cts = null;
        }
    }

    private void HandleHeatModifierChanged(float newValue) {
        this.heatModifer.text = $"Heat Modifier: {newValue}";
    }
}
