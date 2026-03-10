using UnityEngine;

public class LightSensor : MonoBehaviour
{
    [Header("=== References ===")]
    [SerializeField] private LayerMask occlusionMask; // Layermask for raycast to check if walls block the light

    [System.Obsolete("Hard coded solution for now.")]
    [SerializeField] private BaseNavAIMonster stalkerRef; // Reference to the monster AI for stun callback
    
    [Header("=== Configuration ===")]
    [SerializeField] private float exposureBuildSpeed = 3f; // Rate of exposure increase per second when in light
    [SerializeField] private float exposureDecaySpeed = 2f; // Rate of exposure decrease per second when not in light
    [SerializeField] private float stunThreshold = 1f; // Exposure level (0-1) required to trigger stun
    [SerializeField] private float tickInterval = 0.2f; // Seconds between sensor checks (lower = more responsive, higher = better performance)
    [SerializeField] private float sensorCooldownDuration = 5f; // Seconds of immunity after being stunned
    
    [System.Obsolete("temporary internal timer")]
    private Timer performanceTimer;
    private FlashLight flashlight;
    private Transform sensorTransform;
    private Transform flashlightTransform;
    private float remainingCooldownTime;
    private float exposure; // Current exposure level (0-1), visible in inspector for debugging      

    private void Awake()
    {
        this.sensorTransform = this.transform;
    }

    /// <summary>                                                                                                                                                            
    /// Initializes the sensor timer and begins periodic light detection.                                                                                                    
    /// </summary>
    private void Start()
    {
        this.performanceTimer = new Timer(tickInterval, 0);
        this.performanceTimer.OnTimerTick += Sense;
        this.performanceTimer.Start();
    }

    /// <summary>                                                                                                                                                            
    /// Cleans up the timer to prevent memory leaks.                                                                                                                         
    /// </summary>
    private void OnDestroy()
    {
        this.performanceTimer?.Dispose();
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
        if (this.flashlightTransform == null || !this.flashlight.PoweredOn)
        {
            AdjustExposure(-this.exposureDecaySpeed);
            return;
        }
        
        Vector3 flashLightPos = this.flashlightTransform.position;
        Vector3 sensorPos = this.sensorTransform.position;
        Vector3 toSensor = sensorPos - flashLightPos;
        float distanceSquared = toSensor.sqrMagnitude;
        
        // Sensor is outside flashlight range      
        if (distanceSquared > this.flashlight.GetRangeSquared())
        {
            AdjustExposure(-this.exposureDecaySpeed);
            return;
        }
        
        Vector3 flashlightForward = this.flashlightTransform.forward;
        float rawDot = Vector3.Dot(flashlightForward, toSensor);
        // Sensor is behind the flashlight
        if (rawDot <= 0f)
        {
            AdjustExposure(-this.exposureDecaySpeed);
            return;
        }
        
        // Sensor is outside the flashlight cone angle                                                                                                                    
        // Uses squared comparison to avoid sqrt: (dot)^2 < (cosThreshold)^2 * dist^2
        if (rawDot * rawDot < this.flashlight.GetCosineThresholdSquared() * distanceSquared)
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
        
        // Sensor is in the light - calculate exposure intensity based on cone position                                                                                      
        // Intensity is higher when closer to the center of the cone
        float distance = Mathf.Sqrt(distanceSquared);
        float dot = rawDot / distance;
        float intensity = (dot - this.flashlight.GetCosineThreshold()) * this.flashlight.GetInverseConeRange();
        
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
    /// <param name="flashLight">The flashlight instance to monitor.</param> 
    public void SetFlashLight(FlashLight flashLight)
    {
        this.flashlight = flashLight;
        this.flashlightTransform = flashLight.transform;
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