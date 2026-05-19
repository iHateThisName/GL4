using UnityEngine;

[System.Serializable]
public struct DetectionConeData
{
    public float RangeSquared;
    public float CosineThreshold;
    public float CosineThresholdSquared;
    public float InverseConeRange;

    public DetectionConeData(float rangeSquared, float cosineThreshold, float cosineThresholdSquared, float inverseConeRange)
    {
        RangeSquared = rangeSquared;
        CosineThreshold = cosineThreshold;
        CosineThresholdSquared = cosineThresholdSquared;
        InverseConeRange = inverseConeRange;
    }
}

[CreateAssetMenu(fileName = "FlashlightSettings", menuName = "TeamSuperSimple/Flashlight Settings", order = 0)]
public class SO_FlashlightSettings : SO_TransformRef
{
    [Header("=== Light Settings ===")]
    [SerializeField] private float startingLightPower = 40f;
    [SerializeField] private float minLightPower = 3f;
    [SerializeField] private float maxLightPower = 140f;

    [Header("=== Range Settings ===")]
    [SerializeField] private float startingLightRange = 6f;
    [SerializeField] private float maxLightRange = 12f;
    [SerializeField] private float minLightRange = 4f;
    [SerializeField] private float detectionAngle = 40;

    [Header("=== Decay Settings ===")]
    [SerializeField] private float lightDecayRate = 1.5f;
    [SerializeField] private float lightDecayTick = 6f;

    [Header("=== Flicker Settings ===")]
    [SerializeField] private float flickerTime;
    [SerializeField] private float flickerInterval = 0.33f;

    [Header("=== Power Settings ===")]
    [SerializeField] private float lowPowerThreshold = 5f;

    [System.NonSerialized] private DetectionConeData detectionConeData;

    public DetectionConeData DetectionCone => detectionConeData;

    public float GetStartingLightPower() => this.startingLightPower;
    public float GetMinLightPower() => this.minLightPower;
    public float GetMaxLightPower() => this.maxLightPower;
    public float GetStartingLightRange() => this.startingLightRange;
    public float GetMaxLightRange() => this.maxLightRange;
    public float GetMinLightRange() => this.minLightRange;
    public float GetDetectionAngle() => this.detectionAngle;
    public float GetLightDecayRate() => this.lightDecayRate;
    public float GetLightDecayTick() => this.lightDecayTick;
    public float GetFlickerTime() => this.flickerTime;
    public float GetFlickerInterval() => this.flickerInterval;
    public float GetLowPowerThreshold() => this.lowPowerThreshold;

    /// <summary>
    /// Calculates and caches detection cone data based on the current runtime range.
    /// </summary>
    public void CalculateDetectionCone(float currentRange)
    {
        float cosThreshold = Mathf.Cos(this.detectionAngle * 0.5f * Mathf.Deg2Rad);
        this.detectionConeData = new DetectionConeData(
            currentRange * currentRange
            , cosThreshold
            , cosThreshold * cosThreshold
            , 1f / (1f - cosThreshold));

        NotifyDataChanged();
    }

    protected override void OnReset()
    {
        base.OnReset(); // Clears Value (flashlight transform)
        detectionConeData = default;
    }
}
