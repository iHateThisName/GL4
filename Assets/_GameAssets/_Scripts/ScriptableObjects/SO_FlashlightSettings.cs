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
public class SO_FlashlightSettings : ScriptableObject
{
    [Header("=== Light Settings ===")]
    // Initial light intensity at startup
    [SerializeField] private float startingLightPower = 40f;

    // Minimum possible light intensity
    [SerializeField] private float minLightPower = 3f;

    // Maximum possible light intensity
    [SerializeField] private float maxLightPower = 140f;

    [Header("=== Range Settings ===")]
    // Initial light beam range
    [SerializeField] private float startingLightRange = 6f;

    // Maximum beam range at full power
    [SerializeField] private float maxLightRange = 12f;

    // Minimum beam range at lowest power
    [SerializeField] private float minLightRange = 4f;

    // Detection angle for detection cone
    [SerializeField] private float detectionAngle = 40;

    [Header("=== Decay Settings ===")]
    // How much intensity is lost per decay tick
    [SerializeField] private float lightDecayRate = 1.5f;

    // Maximum time between decay ticks
    [SerializeField] private float lightDecayTick = 6f;

    [Header("=== Flicker Settings ===")]
    // How long to flicker the flashlight
    [SerializeField] private float flickerTime;
    // Time between flickering off and on
    [SerializeField] private float flickerInterval = 0.33f;

    [Header("=== Power Settings ===")]
    // Threshold considered "low power"
    [SerializeField] private float lowPowerThreshold = 5f;

    // Maximum number of full crank rotations allowed
    [SerializeField] private int maxRotations = 10;
    
    private static Transform flashlightTransform;
    
    private DetectionConeData detectionConeData;
    
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

    public int GetMaxRotations() => this.maxRotations;
    
    public Transform GetFlashlightTransform() => flashlightTransform;

    /// <summary>
    /// Calculates detection cone data based on the current runtime range.
    /// </summary>
    public void CalculateDetectionCone(float currentRange)
    {
        float cosThreshold = Mathf.Cos(this.detectionAngle * 0.5f * Mathf.Deg2Rad);
        this.detectionConeData = new DetectionConeData(
            currentRange * currentRange
            , cosThreshold
            , cosThreshold * cosThreshold
            , 1f / (1f - cosThreshold));
    }
    
    // Detection cone data for LightSensor queries
    public DetectionConeData GetDetectionCone() => this.detectionConeData;

    public static void SetFlashlightTransform(Transform transform)
    {
        if (flashlightTransform != null) return;
        flashlightTransform = transform;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Reset()
    {
        flashlightTransform = null;
    }
}