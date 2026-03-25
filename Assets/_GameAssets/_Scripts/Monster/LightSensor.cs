using UnityEngine;

public class LightSensor : MonoBehaviour
{
    [Header("=== References ===")]
    [SerializeField] private LayerMask occlusionMask; // Layermask for raycast to check if walls block the light

    [System.Obsolete("Hard coded solution for now.")]
    [SerializeField] private BaseNavAIMonster stalkerRef; // Reference to the monster AI for stun callback
    
    [SerializeField] private SO_FlashlightSettings flashlightSettings;
    
    [Header("=== Configuration ===")]
    [SerializeField] private float exposureBuildSpeed = 3f; // Rate of exposure increase per second when in light
    [SerializeField] private float exposureDecaySpeed = 2f; // Rate of exposure decrease per second when not in light
    [SerializeField] private float stunThreshold = 1f; // Exposure level (0-1) required to trigger stun
    [SerializeField] private float tickInterval = 0.2f; // Seconds between sensor checks (lower = more responsive, higher = better performance)
    [SerializeField] private float sensorCooldownDuration = 5f; // Seconds of immunity after being stunned
    
    [System.Obsolete("temporary internal timer")]
    private TimerHandle performanceTimerHandle;
    private DetectionConeData detectionData;
    private Transform flashlightTransform;
    private Transform sensorTransform;
    private float remainingCooldownTime;
    private float exposure; // Current exposure level (0-1), visible in inspector for debugging

    private void Awake()
    {
        this.sensorTransform = this.transform;
        this.flashlightTransform = null; //this.flashlightSettings?.GetFlashlightTransform();
    }

    /// <summary>                                                                                                                                                            
    /// Initializes the sensor timer and begins periodic light detection.                                                                                                    
    /// </summary>
    private void Start()
    {
        this.performanceTimerHandle = TimerManager.Create(tickInterval);
        TimerManager.SetCallbacks(this.performanceTimerHandle, Sense, null);
    }

    private void OnDestroy()
    {
        TimerManager.Release(ref this.performanceTimerHandle);
    }

    /// <summary>                                                                                                                                                            
    /// Main sensing logic called every tick interval.                                                                                                                       
    /// Checks if the sensor is within the flashlight cone and not occluded,                                                                                                 
    /// then adjusts exposure accordingly.                                                                                                                                   
    /// </summary>
    private void Sense()
    {
        // Handle cooldown - skip all processing during stun immunity  
        if (this.remainingCooldownTime > 0)
        {
            this.remainingCooldownTime -= this.tickInterval;
            return;
        }
        
        // No valid flashlight or flashlight is off
        if (this.flashlightTransform == null)
        {
            AdjustExposure(-this.exposureDecaySpeed);
            return;
        }

        // Get detection cone from flashlight (uses runtime range)
        this.detectionData = new DetectionConeData();//flashlightSettings.GetDetectionCone();

        Vector3 flashLightPos = this.flashlightTransform.position;
        Vector3 sensorPos = this.sensorTransform.position;
        Vector3 toSensor = sensorPos - flashLightPos;
        float distanceSquared = toSensor.sqrMagnitude;

        // Sensor is outside flashlight range
        if (distanceSquared > this.detectionData.RangeSquared)
        {
            AdjustExposure(-this.exposureDecaySpeed);
            return;
        }

        // Flatten to XZ plane (ignore vertical angle)
        Vector3 flashlightForward = this.flashlightTransform.forward;
        flashlightForward.y = 0f;
        flashlightForward.Normalize();

        Vector3 toSensorFlat = toSensor;
        toSensorFlat.y = 0f;
        float flatDistanceSquared = toSensorFlat.sqrMagnitude;

        float rawDot = Vector3.Dot(flashlightForward, toSensorFlat);
        // Sensor is behind the flashlight (horizontally)
        if (rawDot <= 0f)
        {
            AdjustExposure(-this.exposureDecaySpeed);
            return;
        }

        // Sensor is outside the flashlight cone angle (horizontal only)
        // Uses squared comparison to avoid sqrt: (dot)^2 < (cosThreshold)^2 * dist^2
        if (rawDot * rawDot < this.detectionData.CosineThresholdSquared * flatDistanceSquared)
        {
            AdjustExposure(-this.exposureDecaySpeed);
            return;
        }

        // Sensor is occluded by geometry (wall, obstacle, etc.)
        if (Physics.Linecast(flashLightPos, sensorPos, this.occlusionMask))
        {
            AdjustExposure(-this.exposureDecaySpeed);
            return;
        }

        // Sensor is in the light - calculate exposure intensity based on cone position (horizontal)
        // Intensity is higher when closer to the center of the cone
        float flatDistance = Mathf.Sqrt(flatDistanceSquared);
        float dot = rawDot / flatDistance;
        float intensity = (dot - this.detectionData.CosineThreshold) * this.detectionData.InverseConeRange;
        
        // Build exposure based on intensity
        AdjustExposure(intensity * this.exposureBuildSpeed);

        // Check for stun threshold
        if (this.exposure >= this.stunThreshold)
            Stun();
    }
    
    /// <summary>                                                                                                                                                            
    /// Clamps exposure to new value over time                                                                                                         
    /// </summary>
    private void AdjustExposure(float rate)
    {
        this.exposure = Mathf.Clamp01(this.exposure + rate * this.tickInterval);
    }
    
    /// <summary>                                                                                                                                                            
    /// Triggers the stun effect on the monster and starts the cooldown period.                                                                                              
    /// Resets exposure to zero to prevent immediate re-stun after cooldown.                                                                                                 
    /// </summary> 
    private void Stun()
    {
        this.remainingCooldownTime = this.sensorCooldownDuration;
        this.exposure = 0f;
        this.stalkerRef.OnFlashlightHit(this.flashlightTransform.position);
    }

    /// <summary>                                                                                                                                                            
    /// Assigns the flashlight reference for this sensor to track.                                                                                                           
    /// Should be called when the player spawns or when the flashlight becomes available.                                                                                    
    /// </summary>
    public void SetFlashLight(Transform flashlight)
    {
        this.flashlightTransform = flashlight.transform;
    }

    /// <summary>                                                                                                                                                            
    /// Draws a debug gizmo showing current exposure level.                                                                                                                  
    /// Color transitions from green (no exposure) to red (full exposure).                                                                                                   
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.Lerp(Color.green, Color.red, exposure);
        Gizmos.DrawWireSphere(transform.position, 0.15f);
    }
}