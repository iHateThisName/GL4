using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.VRTemplate;

public class RadioFrequencyDisplayController : MonoBehaviour
{
    /* =========================
     * Serialized Fields
     * ========================= */

    [Header("References")]
    [SerializeField] private XRKnob Knob;
    [SerializeField] private TMP_Text FrequencyText;

    [Header("Frequency Range")]
    [SerializeField] private float MinFrequency = 88.0f;
    [SerializeField] private float MaxFrequency = 108.0f;

    [Header("Knob Settings")]
    [Tooltip("Number of full knob rotations required to sweep the full frequency range")]
    [SerializeField] private float RotationsPerFullSweep = 1f;

    [Header("Display Settings")]
    [Range(0, 3)]
    [SerializeField] private int DecimalPlaces = 1;

    /* =========================
     * Private Fields
     * ========================= */

    private float accumulatedTurns;
    private float lastKnobValue;

    /* =========================
     * Unity Lifecycle Methods
     * ========================= */

    private void OnEnable()
    {
        this.lastKnobValue = this.Knob.value;

        // Subscribe to knob events
        this.Knob.onValueChange.AddListener(this.OnKnobValueChanged);
        this.Knob.selectEntered.AddListener(this.OnKnobSelectEntered);

        // 🔑 Initial UI sync so text updates immediately
        this.UpdateFrequencyDisplay();
    }

    private void OnDisable()
    {
        // Unsubscribe from knob events
        this.Knob.onValueChange.RemoveListener(this.OnKnobValueChanged);
        this.Knob.selectEntered.RemoveListener(this.OnKnobSelectEntered);
    }

    /* =========================
     * XR Knob Callbacks
     * ========================= */

    private void OnKnobValueChanged(float currentValue)
    {
        float delta = currentValue - this.lastKnobValue;

        // Handle wraparound (0 → 1 or 1 → 0)
        if (delta > 0.5f)
        {
            delta -= 1f;
        }
        else if (delta < -0.5f)
        {
            delta += 1f;
        }

        this.accumulatedTurns += delta;
        this.lastKnobValue = currentValue;

        this.UpdateFrequencyDisplay();
    }

    private void OnKnobSelectEntered(SelectEnterEventArgs args)
    {
        // Re-sync lastKnobValue and accumulatedTurns when the knob is grabbed
        // Preserve nearest whole-number of full sweeps
        float knobValue = this.Knob.value;
        float nearestFullRotations = Mathf.Round(this.accumulatedTurns);
        this.accumulatedTurns = nearestFullRotations + knobValue;
        this.lastKnobValue = knobValue;

        // 🔑 Update UI immediately when knob is grabbed
        this.UpdateFrequencyDisplay();
    }

    /* =========================
     * Frequency Display
     * ========================= */

    private void UpdateFrequencyDisplay()
    {
        float normalizedValue = Mathf.Repeat(
            this.accumulatedTurns / this.RotationsPerFullSweep,
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
