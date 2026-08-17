using UnityEngine;
using TMPro;
using UnityEngine.XR.Interaction.Toolkit;
using Unity.VRTemplate;

public class RadioFrequencyDisplayController : MonoBehaviour
{
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


    private float accumulatedTurns;
    private float lastKnobValue;


    private void OnEnable()
    {
        this.lastKnobValue = this.Knob.value;

        this.Knob.onValueChange.AddListener(this.OnKnobValueChanged);
        this.Knob.selectEntered.AddListener(this.OnKnobSelectEntered);

        this.UpdateFrequencyDisplay();
    }

    private void OnDisable()
    {
        this.Knob.onValueChange.RemoveListener(this.OnKnobValueChanged);
        this.Knob.selectEntered.RemoveListener(this.OnKnobSelectEntered);
    }

   

    private void OnKnobValueChanged(float currentValue)
    {
        float delta = currentValue - this.lastKnobValue;

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
        float knobValue = this.Knob.value;
        float nearestFullRotations = Mathf.Round(this.accumulatedTurns);
        this.accumulatedTurns = nearestFullRotations + knobValue;
        this.lastKnobValue = knobValue;

        this.UpdateFrequencyDisplay();
    }

    

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
