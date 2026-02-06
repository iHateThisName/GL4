using UnityEngine;
using TMPro;

public class RadioFrequencyDisplayController : MonoBehaviour
{
    /* =========================
     * Serialized Fields
     * ========================= */

    [Header("References")]
    [SerializeField] private Transform KnobTransform;
    [SerializeField] private TMP_Text FrequencyText;

    [Header("Frequency Range")]
    [SerializeField] private float MinFrequency = 88.0f;
    [SerializeField] private float MaxFrequency = 108.0f;

    [Header("Knob Settings")]
    [Tooltip("Degrees of rotation required to sweep the full frequency range")]
    [SerializeField] private float DegreesPerFullSweep = 360f;

    [Header("Display Settings")]
    [Range(0, 3)]
    [SerializeField] private int DecimalPlaces = 1;

    /* =========================
     * Private Fields
     * ========================= */

    private float accumulatedRotation;
    private float lastKnobAngle;

    /* =========================
     * Unity Lifecycle Methods
     * ========================= */

    private void Start()
    {
        this.InitializeKnobRotation();
    }

    private void Update()
    {
        this.TrackKnobRotation();
        this.UpdateFrequencyDisplay();
    }

    /* =========================
     * Initialization
     * ========================= */

    private void InitializeKnobRotation()
    {
        this.lastKnobAngle = this.KnobTransform.localEulerAngles.y;
        this.accumulatedRotation = 0f;
    }

    /* =========================
     * Knob Logic
     * ========================= */

    private void TrackKnobRotation()
    {
        float currentKnobAngle = this.KnobTransform.localEulerAngles.y;
        float deltaRotation = Mathf.DeltaAngle(this.lastKnobAngle, currentKnobAngle);

        this.accumulatedRotation += deltaRotation;
        this.lastKnobAngle = currentKnobAngle;
    }

    /* =========================
     * Frequency Display
     * ========================= */

    private void UpdateFrequencyDisplay()
    {
        float normalizedValue = Mathf.Repeat(
            this.accumulatedRotation / this.DegreesPerFullSweep,
            1f
        );

        float frequency = Mathf.Lerp(
            this.MinFrequency,
            this.MaxFrequency,
            normalizedValue
        );

        this.FrequencyText.text = frequency.ToString($"F{this.DecimalPlaces}");
    }
}
