using System.Collections;
using UnityEngine;

public class FMODFrequencyParameter : MonoBehaviour {
    [SerializeField, Tooltip("Minimum value of the FMOD parameter")]
    private float minValue = 0f;
    [SerializeField, Tooltip("Maximum value of the FMOD parameter")]
    private float maxValue = 3f;
    [SerializeField, Tooltip("Name of the FMOD parameter to control")]
    private string parameterName = "Frequency";
    [SerializeField, Tooltip("Speed at which the parameter changes")]
    private float animationSpeed = 0.1f;
    [SerializeField, Tooltip("Amount by which the parameter changes each step")]
    private float changeAmount = 0.1f;

    [SerializeField, Tooltip("Target value of the FMOD parameter, The value to change")]
    private float _targetValue;

    public float TargetValue { // The value you should get changed by other scripts.
        get => _targetValue;
        set {
            _targetValue = value;
            ApplyValueChanges();
        }
    }

    private FMODUnity.StudioEventEmitter emitter;
    private float currentValue = 0f;
    private bool isAnimating = false;

    private void Awake() {
        if (emitter == null) {
            emitter = GetComponent<FMODUnity.StudioEventEmitter>();
            if (emitter == null) {
                Debug.LogError($"{nameof(FMODFrequencyParameter)}: No StudioEventEmitter found on the GameObject.");
            }
        }
    }

    private void OnValidate() {
        ApplyValueChanges();
    }

    private void ApplyValueChanges() {
        this._targetValue = Mathf.Clamp(this._targetValue, this.minValue, this.maxValue);
        if (this._targetValue != this.currentValue && !this.isAnimating) {
            StartCoroutine(AnimateParameterChange());
        }
    }

    private IEnumerator AnimateParameterChange() {
        this.isAnimating = true; // flag to prevent multiple coroutines from running simultaneously

        while (this._targetValue != this.currentValue) {
            yield return new WaitForSeconds(this.animationSpeed);

            if (this.currentValue == this.TargetValue) yield break;

            if (this.TargetValue > this.currentValue) {
                this.currentValue += this.changeAmount;
                if (this.currentValue > this.TargetValue) this.currentValue = this.TargetValue;
            } else {
                this.currentValue -= this.changeAmount;
                if (this.currentValue < this.TargetValue) this.currentValue = this.TargetValue;
            }
            this.emitter.SetParameter(this.parameterName, this.currentValue);
        }

        this.isAnimating = false; // reset flag when done
    }
}
