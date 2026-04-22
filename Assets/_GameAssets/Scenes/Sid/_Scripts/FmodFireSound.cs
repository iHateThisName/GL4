using FMODUnity;
using UnityEngine;

public class FmodFireSound : MonoBehaviour
{
    [SerializeField] private StudioEventEmitter emitter;
    [SerializeField] private string parameterName = "WindChaseState";

    [SerializeField, Range(0f, 3f)]
    [Tooltip("Slider limited 0.00 - 3.00. Value is rounded to 2 decimal places.")]
    private float parameterSlider = 0f;
    private float previousSliderValue = 0f;

    // Ensure value stays within range and has max 2 decimal places in the inspector
    private void OnValidate()
    {
        this.parameterSlider = Mathf.Clamp(this.parameterSlider, 0f, 3f);
        this.parameterSlider = Mathf.Round(this.parameterSlider * 100f) / 100f;

        if (this.previousSliderValue != this.parameterSlider)
        {
            this.emitter.SetParameter(this.parameterName, this.parameterSlider);
            this.previousSliderValue = this.parameterSlider;
            Debug.Log($"Parameter '{this.parameterName}' set to {this.parameterSlider}");
        }
    }

    [ContextMenu("Debug Test")]
    public void DebugTest()
    {
        this.emitter.SetParameter(this.parameterName, this.parameterSlider);
    }
}
