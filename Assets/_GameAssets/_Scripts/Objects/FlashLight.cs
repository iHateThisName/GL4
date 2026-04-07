using Assets.Scripts.Singleton;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

/// <summary>
/// Simulates a crank-powered flashlight.
/// 
/// The flashlight:
/// • Gains power when the crank is rotated
/// • Gradually loses power over time
/// • Automatically turns off when power reaches minimum
/// • Adjusts light intensity and range based on remaining power
/// </summary>
[RequireComponent(typeof(XRGrabInteractable))] 
// Ensures the object always has an XRGrabInteractable component
public class Flashlight : Singleton<Flashlight>
{
    [Header("=== References ===")]
    // Handle used to grab and toggle the flashlight on/off
    [SerializeField] private XRGrabInteractable grabInteractable;

    // Crank used to generate flashlight power
    [SerializeField] private RotationableInteractable crankInteractable;

    // Unity Light component controlling beam visuals
    [SerializeField] private Light lightSource;
    
    [Header("=== Light Settings ===")]
    [SerializeField] private SO_FlashlightSettings flashlightSettings;
    
    // Debug/testing: start flashlight enabled
    [System.Obsolete("Only for testing purposes.")]
    [SerializeField] private bool startEnabled = false;

    private TimerHandle batteryHandle;
    
    // Target intensity we smoothly move toward
    private float targetLightIntensity;

    // Target range (derived from intensity)
    private float targetLightRange;
    
    // Current partial crank angle (0–360 range)
    private float currentAngle;

    // Total number of full crank rotations completed
    private int fullRotations;

    // Whether flashlight is currently turned on
    private bool powered;
    
    // Whether the flashlight should flicker on or off this frame/flicker
    private bool flickeredLastFrame;

    // How much intensity one full crank rotation adds
    private const float LIGHT_MAGNITUDE = 10;

    // ==== Unity Lifecycle ====
    #region Unity Lifecycle
    /// <summary>
    /// Automatically fetch Light component if not set in inspector.
    /// </summary>
    protected override void Awake()
    {
        base.Awake();
        if (this.lightSource == null)
            this.lightSource = GetComponentInChildren<Light>();
    }

    /// <summary>
    /// Subscribes to crank and grab events.
    /// </summary>
    private void OnEnable()
    {
        // Subscribe to crank rotation event
        if (this.crankInteractable != null) 
            this.crankInteractable.OnCrank += OnCrankRotated;
        
        // Subscribe to handle grab events (on/off toggle)
        if (this.grabInteractable != null)
        {
            this.grabInteractable.selectEntered.AddListener(OnFlashlightPickedup);
            this.grabInteractable.selectExited.AddListener(OnFlashlightDropped);
        }
    }

    /// <summary>
    /// Unsubscribes from events to prevent leaks.
    /// </summary>
    private void OnDisable()
    {
        if (this.crankInteractable != null) 
            this.crankInteractable.OnCrank -= OnCrankRotated;

        if (this.grabInteractable != null)
        {
            this.grabInteractable.selectEntered.RemoveListener(OnFlashlightPickedup);
            this.grabInteractable.selectExited.RemoveListener(OnFlashlightDropped);
        }
    }

    /// <summary>
    /// Initializes flashlight values.
    /// </summary>
    private void Start()
    {
        this.powered = false;
        
        if (this.lightSource != null)
        {
            this.targetLightIntensity = this.flashlightSettings.GetStartingLightPower();
            this.targetLightRange = this.flashlightSettings.GetStartingLightRange();
            this.lightSource.intensity = this.targetLightIntensity;
            this.lightSource.range = this.targetLightRange;
        }
        this.flashlightSettings.Value = this.transform;
        this.flashlightSettings.CalculateDetectionCone(this.targetLightRange);

        // Ensure flashlight starts off
        ToggleFlashLight(false);

        // Optional debug start
        if (startEnabled) ToggleFlashLight(true);

        SetupActiveFlashlightTimer();
    }

    private void OnDestroy()
    {
        TimerManager.Release(ref this.batteryHandle);
    }
    #endregion

    private void OnFlashlightDecay()
    {
        // If power is too low, turn off
        if (this.LightIntensity <= this.flashlightSettings.GetMinLightPower())
        {
            ToggleFlashLight(false);
            return;
        }
        // Reduce intensity
        UpdateLightIntensity(-this.flashlightSettings.GetLightDecayRate());
        UpdateFlashLight();
    }
    
    /// <summary>
    /// Adjusts target light intensity while clamping to limits.
    /// </summary>
    private void UpdateLightIntensity(float delta)
    {
        float clampedLightIntensity = Mathf.Clamp(this.targetLightIntensity + delta, this.flashlightSettings.GetMinLightPower(), this.flashlightSettings.GetMaxLightPower());
        this.targetLightIntensity = clampedLightIntensity;
    }

    /// <summary>
    /// Smoothly interpolates intensity and updates beam range.
    /// </summary>
    private void UpdateFlashLight()
    {
        if (this.lightSource == null) return;

        // Smooth transition toward target intensity
        this.lightSource.intensity = Mathf.MoveTowards(this.LightIntensity, this.targetLightIntensity, 5f * Time.deltaTime);

        // Normalize intensity to 0–1 range
        float normalized = Mathf.InverseLerp(this.flashlightSettings.GetMinLightPower(), this.flashlightSettings.GetMaxLightPower(), this.LightIntensity);

        // Adjust beam range based on power level
        this.targetLightRange = Mathf.Lerp(this.flashlightSettings.GetMinLightRange(), this.flashlightSettings.GetMaxLightRange(), normalized);
        this.lightSource.range = Mathf.MoveTowards(this.lightSource.range, this.targetLightRange, 5f * Time.deltaTime);

        // Update detection cone with new range
        this.flashlightSettings.CalculateDetectionCone(this.targetLightRange);
    }
    
    // ==== Crank Logic ====

    /// <summary>
    /// Called whenever the crank rotates.
    /// Converts rotation into power.
    /// </summary>
    void OnCrankRotated(float delta)
    {
        // Prevent exceeding max allowed rotations
        if (this.fullRotations >= this.flashlightSettings.GetMaxRotations()) return;

        this.currentAngle += delta;

        // Apply partial power based on rotation (gives immediate feedback while cranking)
        float partialPower = (delta / 360f) * LIGHT_MAGNITUDE;
        UpdateLightIntensity(partialPower);

        // Count full rotations for tracking purposes
        while (this.currentAngle >= 360f)
        {
            this.currentAngle -= 360f;
            this.fullRotations++;
        }

        while (this.currentAngle <= -360f)
        {
            this.currentAngle += 360f;
            this.fullRotations--;
        }
        
        UpdateFlashLight();
    }
    
    // ==== Flashlight Helpers ====

    private void OnFlashlightDropped(SelectExitEventArgs args)
    {
        if (args.interactorObject == null || this.LightIntensity <= this.flashlightSettings.GetMinLightPower())
            SetupDroppedFlashlightTimer();
    }

    /// <summary>
    /// Called when flashlight is grabbed. Ensures it always ends up in the left hand.
    /// </summary>
    private void OnFlashlightPickedup(SelectEnterEventArgs args)
    {
        ToggleOnFlashlight();
    }

    private void OnFlashlightFlicker()
    {
        flickeredLastFrame = !flickeredLastFrame;
        ToggleFlashLight(flickeredLastFrame);
    }
    
    private void SetupActiveFlashlightTimer()
    {
        if (!TimerManager.Validate(this.batteryHandle))
            this.batteryHandle = TimerManager.Create(this.flashlightSettings.GetLightDecayTick());
        else
            TimerManager.Reconfigure(this.batteryHandle, this.flashlightSettings.GetLightDecayTick());

        TimerManager.SetCallbacks(this.batteryHandle, OnFlashlightDecay, null);
    }

    private void SetupDroppedFlashlightTimer()
    {
        if (!TimerManager.Validate(this.batteryHandle))
            this.batteryHandle = TimerManager.Create(0, 5);
        else
            TimerManager.Reconfigure(this.batteryHandle, 0, 5);

        // Timer with interval 0 won't tick — it just waits for duration to finish
        TimerManager.SetCallbacks(this.batteryHandle, null, SetupFlashlightFlickerTimer);
    }

    private void SetupFlashlightFlickerTimer()
    {
        if (!TimerManager.Validate(this.batteryHandle))
            this.batteryHandle = TimerManager.Create(this.flashlightSettings.GetFlickerInterval(), this.flashlightSettings.GetFlickerTime());
        else
            TimerManager.Reconfigure(this.batteryHandle, this.flashlightSettings.GetFlickerInterval(), this.flashlightSettings.GetFlickerTime());

        TimerManager.SetCallbacks(this.batteryHandle, OnFlashlightFlicker, ResumeFlashlightAfterFlicker);
    }

    private void ResumeFlashlightAfterFlicker()
    {
        this.flickeredLastFrame = false;
        ToggleFlashLight(true);
        
        if (this.LightIntensity < this.flashlightSettings.GetMinLightPower())
        {
            ToggleFlashLight(false);
        }
        SetupActiveFlashlightTimer();
    }
    
    /// <summary>
    /// Turns flashlight on when grabbed.
    /// </summary>
    private void ToggleOnFlashlight()
    {
        ToggleFlashLight(true);
        TimerManager.Resume(this.batteryHandle);
    }

    private void ToggleOffFlashlight()
    {
        ToggleFlashLight(false);
        TimerManager.Pause(this.batteryHandle);
    }
    
    /// <summary>
    /// Enables or disables the light component.
    /// </summary>
    private void ToggleFlashLight(bool toggle)
    {
        this.powered = toggle;

        if (this.lightSource != null)
            this.lightSource.enabled = this.powered;
    }

    #region Getters
    // public getter for powered state
    public bool PoweredOn => this.powered;

    // Whether flashlight is in low power state
    public bool HasLowPower => this.LightIntensity <= this.flashlightSettings.GetLowPowerThreshold();

    // Current visible light intensity
    public float LightIntensity => this.lightSource != null ? this.lightSource.intensity : this.flashlightSettings.GetStartingLightPower();
    #endregion
    
#if UNITY_EDITOR
    [ContextMenu("Update Light Intensity")]
    private void TestLightIntensity()
    {
        UpdateLightIntensity(this.flashlightSettings.GetStartingLightPower());
        UpdateFlashLight();
    }
    
    [ContextMenu("Test Flicker")]
    private void TestFlicker()
    {
        SetupFlashlightFlickerTimer();
    }

    [ContextMenu("Test Drop")]
    private void TestDrop()
    {
        OnFlashlightDropped(null);
    }

    [ContextMenu("Test Pickup")]
    private void TestPickup()
    {
        ToggleOnFlashlight();
    }
    
    [ContextMenu("Test Crank")]
    private void TestCrank()
    {
        OnCrankRotated(180);
    }
    
    private void OnDrawGizmos()
    {
        Vector3 origin = this.transform.position;
        Vector3 forward = this.transform.forward;
        float halfAngle = this.flashlightSettings ? this.flashlightSettings.GetDetectionAngle() * 0.5f : 3;

        // Reference edge: forward tilted by halfAngle
        Vector3 baseEdge = Quaternion.AngleAxis(halfAngle, this.transform.right) * forward;

        const int rimSegments = 32;
        const int edgeLines = 8;

        // Cone edge lines
        Gizmos.color = Color.yellow;
        for (int i = 0; i < edgeLines; i++)
        {
            float azimuth = (360f / edgeLines) * i;
            Vector3 edgeDir = Quaternion.AngleAxis(azimuth, forward) * baseEdge;
            Gizmos.DrawRay(origin, edgeDir * this.targetLightRange);
        }

        // Rim circle at range
        Vector3 prevPoint = origin + baseEdge * this.targetLightRange;
        for (int i = 1; i <= rimSegments; i++)
        {
            float azimuth = (360f / rimSegments) * i;
            Vector3 edgeDir = Quaternion.AngleAxis(azimuth, forward) * baseEdge;
            Vector3 point = origin + edgeDir * this.targetLightRange;
            Gizmos.DrawLine(prevPoint, point);
            prevPoint = point;
        }

        // Center forward line
        Gizmos.color = Color.white;
        Gizmos.DrawRay(origin, forward * this.targetLightRange);
    }
#endif
}
