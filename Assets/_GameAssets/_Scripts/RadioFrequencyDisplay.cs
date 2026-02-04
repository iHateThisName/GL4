using UnityEngine;
using TMPro;

public class RadioFrequencyDisplay : MonoBehaviour
{
    /* =========================
     * Serialized Fields
     * ========================= */

    [Header("References")]
    [SerializeField] private Transform knobTransform;
    [SerializeField] private TMP_Text frequencyText;

    [Header("Knob Rotation Range")]
    [Tooltip("Minimum local Y rotation of the knob (degrees)")]
    [SerializeField] private float minKnobAngle = -135f;

    [Tooltip("Maximum local Y rotation of the knob (degrees)")]
    [SerializeField] private float maxKnobAngle = 135f;

    [Header("Frequency Range")]
    [SerializeField] private float minFrequency = 88.0f;
    [SerializeField] private float maxFrequency = 108.0f;

    [Header("Display Settings")]
    [SerializeField] private int decimalPlaces = 1;

    /* =========================
     * Unity Lifecycle Methods
     * ========================= */

    private void Update()
    {
        float knobValue = GetNormalizedKnobValue();
        UpdateFrequencyDisplay(knobValue);
    }

    /* =========================
     * Private Methods
     * ========================= */

    private float GetNormalizedKnobValue()
    {
        float currentAngle = this.knobTransform.localEulerAngles.y;

        // Convert 0–360 to -180–180
        currentAngle = Mathf.DeltaAngle(0f, currentAngle);

        return Mathf.InverseLerp(
            this.minKnobAngle,
            this.maxKnobAngle,
            currentAngle
        );
    }

    private void UpdateFrequencyDisplay(float knobValue)
    {
        float frequency = Mathf.Lerp(
            this.minFrequency,
            this.maxFrequency,
            knobValue
        );

        this.frequencyText.text =
            frequency.ToString($"F{this.decimalPlaces}");
    }
}
