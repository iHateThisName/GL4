using System;
using UnityEngine;

public class LightSensor : MonoBehaviour
{
    [Header("=== References ===")]
    // Layermask for optional raycast check if walls should block the light
    [SerializeField] private LayerMask occlusionMask;

    [System.Obsolete("Hard coded solution for now.")]
    [SerializeField] private BaseNavAIMonster stalkerRef;
    
    [SerializeField] private float exposureBuildSpeed = 3f;
    [SerializeField] private float exposureDecaySpeed = 2f;
    [SerializeField] private float exposure;
    [SerializeField] private float stunThreshold = 1f;
    [SerializeField] private float tickInterval = 0.2f;
    [SerializeField] private float sensorCooldownDuration = 5f;
    
    [System.Obsolete("temporary internal timer")]
    private Timer performanceTimer;
    private FlashLight flashLight;
    private Transform sensorTransform;
    private Transform flashLightTransform;
    private float remainingCooldownTime;

    private void Awake()
    {
        this.sensorTransform = this.transform;
    }

    private void Start()
    {
        if (this.flashLight != null) this.flashLightTransform = this.flashLight.transform;
        
        this.performanceTimer = new Timer(tickInterval, 0);
        this.performanceTimer.OnTimerTick += Sense;
        this.performanceTimer.Start();
    }

    private void OnDestroy()
    {
        if (this.performanceTimer != null) 
            this.performanceTimer.Dispose();
    }

    private void Sense()
    {
        if (this.remainingCooldownTime > 0)
        {
            this.remainingCooldownTime -= this.tickInterval;
        }
        
        if (this.flashLightTransform == null || !this.flashLight.PoweredOn)
        {
            AdjustExposure(-this.exposureDecaySpeed);
            return;
        }
        
        Vector3 flashLightPos = this.flashLightTransform.position;
        Vector3 sensorPos = this.sensorTransform.position;
        Vector3 toSensor = sensorPos - flashLightPos;
        
        float distSq = toSensor.sqrMagnitude;
        if (distSq > this.flashLight.GetRangeSquared())
        {
            AdjustExposure(-this.exposureDecaySpeed);
            return;
        }
        
        Vector3 flashFwd = this.flashLightTransform.forward;
        float rawDot = Vector3.Dot(flashFwd, toSensor);
        if (rawDot <= 0f)
        {
            AdjustExposure(-this.exposureDecaySpeed);
            return;
        }
        
        if (rawDot * rawDot < this.flashLight.GetCosineThresholdSquared() * distSq)
        {
            AdjustExposure(-this.exposureDecaySpeed);
            return;
        }
        
        if (Physics.Linecast(flashLightPos, sensorPos, this.occlusionMask))
        {
            AdjustExposure(-this.exposureDecaySpeed);
        }
        
        float dot = rawDot / Mathf.Sqrt(distSq);
        float intensity = (dot - this.flashLight.GetCosineThreshold()) * this.flashLight.GetInverseConeRange();
        
        AdjustExposure(intensity * this.exposureBuildSpeed);

        if (this.exposure >= this.stunThreshold && this.remainingCooldownTime <= 0)
            Stun();
    }
    
    private void AdjustExposure(float rate)
    {
        this.exposure = Mathf.Clamp01(this.exposure + rate * Time.deltaTime);
    }
    
    private void Stun()
    {
        this.remainingCooldownTime = this.sensorCooldownDuration;
        this.stalkerRef.OnFlashlightHit(this.flashLightTransform.position);
    }

    public void SetFlashLight(FlashLight flashLight)
    {
        this.flashLight = flashLight;
        this.flashLightTransform = flashLight.transform;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.Lerp(Color.green, Color.red, exposure);
        Gizmos.DrawWireSphere(transform.position, 0.15f);
    }
}
