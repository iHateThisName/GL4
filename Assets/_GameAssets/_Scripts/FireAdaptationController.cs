using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class FireAdaptationController : MonoBehaviour
{
    [SerializeField] private Volume volume;
    [SerializeField] private readonly float intensity = 0.5f;

    private Vignette vignette;
    private ColorAdjustments colorAdjustments;

    private void Awake() {
        volume.profile.TryGet<Vignette>(out vignette);
        volume.profile.TryGet<ColorAdjustments>(out colorAdjustments);
    }

    [ContextMenu("Let there be dark")]
    public void ApplyVolume() {
        this.vignette.intensity.value = Mathf.Lerp(0f, 0.35f, intensity);
        this.colorAdjustments.saturation.value = Mathf.Lerp(0f, -20f, intensity);
        this.colorAdjustments.postExposure.value = Mathf.Lerp(0f, -2f, intensity);
    }

    public void RemoveVolume() {
        this.vignette.intensity.value = 0f;
        this.colorAdjustments.saturation.value = 0f;
        this.colorAdjustments.postExposure.value = 0f;
    }
}
