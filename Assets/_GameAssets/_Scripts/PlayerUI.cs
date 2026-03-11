using System.Collections;
using TMPro;
using UnityEngine;
using static PlayerTemperatureSimulator;

/// <summary>
/// Manages the player's UI display
/// </summary>
public class PlayerUI : MonoBehaviour {

    [Header("Refrences")]
    [SerializeField] private TMP_Text temperatureText;
    [SerializeField] private TMP_Text locationText;
    [SerializeField] private TextMeshProUGUI hungerText;
    [SerializeField] private TextMeshProUGUI nightTimeText;

    [Header("Vision Effect Refrences")]
    [SerializeField] private CanvasGroup heatCanvasGroup;
    [SerializeField] private CanvasGroup coldCanvasGroup;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 5f;
    private const float DEFAULT_MEDIUM_EFFECT_ALPHA = 0.125f;
    private const float DEFAULT_HIGH_EFFECT_ALPHA = 0.25f;

    private bool useDebugInfo = false;
    private Coroutine currentWarmFadeCoroutine;
    private Coroutine currentColdFadeCoroutine;

    private void Awake() {
        //#if UNITY_EDITOR
        this.useDebugInfo = true;
        //#endif
    }
    private void Start() {
        if (!this.useDebugInfo) return; // Avoid initializing if we're not in debug mode.

        HandleTemperatureChanged(new BodyTemperatureStateChange {
            CurrentState = PlayerTemperatureSimulator.Instance.CurrentBodyTemperatureState
        });
        HandleHungerChanged(100);
        HandleLocationChanged(PlayerTemperatureSimulator.Instance.CurrentLocationType);

    }

    private void OnEnable() {
        this.temperatureText.text = "";
        this.hungerText.text = "";
        this.locationText.text = "";
        if (!this.useDebugInfo) return; // Avoid subscribing if we're not in debug mode.

        PlayerTemperatureSimulator.OnBodyTemperatureStateChanged += HandleTemperatureChanged;
        PlayerTemperatureSimulator.OnLocationTypeChanged += HandleLocationChanged;
        HungerSystem.OnHungerChanged += HandleHungerChanged;
    }

    private void OnDisable() {
        if (!this.useDebugInfo) return; // Avoid unsubscribing if we never subscribed in the first place.

        PlayerTemperatureSimulator.OnBodyTemperatureStateChanged -= HandleTemperatureChanged;
        PlayerTemperatureSimulator.OnLocationTypeChanged -= HandleLocationChanged;
        HungerSystem.OnHungerChanged -= HandleHungerChanged;
    }

    private void Update() {
        if (this.nightTimeText == null) return;
        this.nightTimeText.text = "Time: " + GameManager.Instance.NightTime.ToString("F2");
    }

    /// <summary>
    /// Handles body temperature state changes by updating the UI text and color.
    /// </summary>
    /// <param name="change">The body temperature state change data.</param>
    private void HandleTemperatureChanged(BodyTemperatureStateChange change) {
        this.temperatureText.text = $"Temperature State: {change.CurrentState}";
        UpdateColor(change.CurrentState);

        if (change.CurrentState == EnumBodyTemperatureState.Normal && change.PreviousState == EnumBodyTemperatureState.MildHyperthermia) {
            StartFadeCanvasGroup(canvasGroup: this.heatCanvasGroup, fadeIn: false);
        } else if (change.CurrentState == EnumBodyTemperatureState.MildHyperthermia) {
            StartFadeCanvasGroup(canvasGroup: this.heatCanvasGroup, fadeIn: true, targetAlpha: DEFAULT_MEDIUM_EFFECT_ALPHA);
        } else if (change.CurrentState == EnumBodyTemperatureState.ModerateHypothermia) {
            StartFadeCanvasGroup(canvasGroup: this.heatCanvasGroup, fadeIn: true, targetAlpha: DEFAULT_HIGH_EFFECT_ALPHA);
        }

        if (change.CurrentState == EnumBodyTemperatureState.Normal && change.PreviousState == EnumBodyTemperatureState.MildHypothermia) {
            StartFadeCanvasGroup(canvasGroup: this.coldCanvasGroup, fadeIn: false);
        } else if (change.CurrentState == EnumBodyTemperatureState.MildHypothermia) {
            StartFadeCanvasGroup(canvasGroup: this.coldCanvasGroup, fadeIn: true, targetAlpha: DEFAULT_MEDIUM_EFFECT_ALPHA);
        } else if (change.CurrentState == EnumBodyTemperatureState.ModerateHypothermia) {
            StartFadeCanvasGroup(canvasGroup: this.coldCanvasGroup, fadeIn: true, targetAlpha: DEFAULT_HIGH_EFFECT_ALPHA);
        }
    }

    /// <summary>
    /// Updates the temperature text color based on the current body temperature state.
    /// </summary>
    /// <param name="state">The current body temperature state.</param>
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
                this.temperatureText.color = Color.magenta; // Extreme conditions Dead.
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
            if (currentBodyTemperatureState == EnumBodyTemperatureState.ModerateHyperthermia) {
                StartFadeCanvasGroup(this.heatCanvasGroup, fadeIn: true, targetAlpha: DEFAULT_HIGH_EFFECT_ALPHA);
            } else {
                StartFadeCanvasGroup(this.heatCanvasGroup, fadeIn: true, targetAlpha: DEFAULT_MEDIUM_EFFECT_ALPHA);
            }
        } else if (currentBodyTemperatureState != EnumBodyTemperatureState.MildHyperthermia || currentBodyTemperatureState != EnumBodyTemperatureState.ModerateHyperthermia) {
            if (!TemperatureZoneManager.Instance.IsPlayerInZone(EnumLocationType.Warm)) {
                StartFadeCanvasGroup(this.heatCanvasGroup, fadeIn: false);
            }
        }
    }

    private void StartFadeCanvasGroup(CanvasGroup canvasGroup, bool fadeIn, float targetAlpha = -1) {
        // Determine if this is the warm effect or cold effect based on the canvas group reference.
        bool isWarmEffect = canvasGroup == this.heatCanvasGroup;

        if (isWarmEffect) {
            if (this.currentWarmFadeCoroutine != null) StopCoroutine(this.currentWarmFadeCoroutine);
            this.currentWarmFadeCoroutine = StartCoroutine(FadeCanvasGroupCoroutine(canvasGroup, fadeIn, targetAlpha));
        } else {
            if (this.currentColdFadeCoroutine != null) StopCoroutine(this.currentColdFadeCoroutine);
            this.currentColdFadeCoroutine = StartCoroutine(FadeCanvasGroupCoroutine(canvasGroup, fadeIn, targetAlpha));
        }
    }

    /// <summary>
    /// Fades a canvas group in or out over a specified duration.
    /// </summary>
    /// <param name="canvasGroup">The canvas group to fade.</param>
    /// <param name="fadeIn">True to fade in, false to fade out.</param>
    /// <returns>IEnumerator for coroutine.</returns>
    private IEnumerator FadeCanvasGroupCoroutine(CanvasGroup canvasGroup, bool fadeIn, float targetAlpha = -1) {
        Debug.Log($"Starting fade {(fadeIn ? "in" : "out")} for {canvasGroup.gameObject.name} to target alpha {targetAlpha}");

        if (canvasGroup == null) yield break;


        if (fadeIn && !canvasGroup.gameObject.activeInHierarchy) {
            // If fading in and the canvas group is not active, enable it before starting the fade.
            canvasGroup.gameObject.SetActive(true);
        }

        float startAlpha = canvasGroup.alpha;
        if (targetAlpha == -1) {
            targetAlpha = fadeIn ? DEFAULT_HIGH_EFFECT_ALPHA : 0f;
        }
        float elapsedTime = 0f;

        while (elapsedTime < this.fadeDuration) {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / this.fadeDuration;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha; // Ensure final alpha is set

        if (!fadeIn) {
            // If fading out, disable the canvas group after the fade is complete
            canvasGroup.gameObject.SetActive(false);
        }
    }
}
