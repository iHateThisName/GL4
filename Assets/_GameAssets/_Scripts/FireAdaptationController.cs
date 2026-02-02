using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FireAdaptationController : MonoBehaviour {
    [Header("Volume")]
    [SerializeField] private Volume volume;
    
    [Header("Effect Parameters")]
    private readonly float intensity = 0.5f;
    private readonly float duration = 5f;
    [SerializeField] private VolumeSettings volumeSettings = new VolumeSettings();
    
    [Header("References")]
    private Vignette vignette;
    private ColorAdjustments colorAdjustments;
    private Coroutine ApplyVolumeCoroutine;

    [System.Serializable]
    private class VolumeSettings {
        public float vignetteIntensity = 0.30f;
        public float saturation = 30f;
        public float exposure = 0.2f;
    }

    /// <summary>
    /// Initializes the volume profile components.
    /// </summary>
    private void Awake() {
        this.volume.profile.TryGet<Vignette>(out this.vignette);
        this.volume.profile.TryGet<ColorAdjustments>(out this.colorAdjustments);
    }

    /// <summary>
    /// Applies the fire adaptation volume effect by gradually transitioning to target settings.
    /// Stops any currently running volume transition before starting a new one.
    /// </summary>
    [ContextMenu("Apply FIRE")]
    public void ApplyVolume() {

        if (this.ApplyVolumeCoroutine != null) {
            StopCoroutine(this.ApplyVolumeCoroutine);
        }
        this.ApplyVolumeCoroutine = StartCoroutine(ApplyVolumeIEnumerator());
    }

    /// <summary>
    /// Removes the fire adaptation volume effect by gradually transitioning back to default settings.
    /// Stops any currently running volume transition before starting a new one.
    /// </summary>
    [ContextMenu("UnApply FIRE")]
    public void RemoveVolume() {
        if (this.ApplyVolumeCoroutine != null) {
            StopCoroutine(this.ApplyVolumeCoroutine);
        }
        this.ApplyVolumeCoroutine = StartCoroutine(RemoveVolumeIEnumerator());
    }

    /// <summary>
    /// Coroutine that gradually applies the fire adaptation effect over the specified duration.
    /// </summary>
    private IEnumerator ApplyVolumeIEnumerator() {
        float elapsed = 0f;

        float startVignette = this.vignette.intensity.value;
        float startSaturation = this.colorAdjustments.saturation.value;
        float startExposure = this.colorAdjustments.postExposure.value;

        while (elapsed < this.duration) {
            elapsed += Time.deltaTime;
            float normalize = elapsed / duration; // Normalized time

            this.vignette.intensity.value = Mathf.Lerp(startVignette, Mathf.Lerp(0f, this.volumeSettings.vignetteIntensity, this.intensity), normalize);
            this.colorAdjustments.saturation.value = Mathf.Lerp(startSaturation, Mathf.Lerp(0f, this.volumeSettings.saturation, this.intensity), normalize);
            this.colorAdjustments.postExposure.value = Mathf.Lerp(startExposure, Mathf.Lerp(0f, this.volumeSettings.exposure, this.intensity), normalize);

            yield return null;
        }

        this.ApplyVolumeCoroutine = null;

    }

    /// <summary>
    /// Coroutine that gradually removes the fire adaptation effect over the specified duration.
    /// </summary>
    private IEnumerator RemoveVolumeIEnumerator() {
        float elapsed = 0f;

        float startVignette = this.vignette.intensity.value;
        float startSaturation = this.colorAdjustments.saturation.value;
        float startExposure = this.colorAdjustments.postExposure.value;

        while (elapsed < this.duration) {
            elapsed += Time.deltaTime;
            float normalize = elapsed / duration; // Normalized time

            this.vignette.intensity.value = Mathf.Lerp(startVignette, 0f, normalize);
            this.colorAdjustments.saturation.value = Mathf.Lerp(startSaturation, 0f, normalize);
            this.colorAdjustments.postExposure.value = Mathf.Lerp(startExposure, 0f, normalize);

            yield return null;
        }
        this.ApplyVolumeCoroutine = null;
    }
}
