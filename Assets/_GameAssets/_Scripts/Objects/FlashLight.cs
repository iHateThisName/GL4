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
public class Flashlight : MonoBehaviour
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

    [Header("=== Drop / Socket Settings ===")]
    // Player transform used to measure drop distance
    [SerializeField] private Transform playerTransform;

    // Socket on the player where the flashlight snaps if dropped too far
    [SerializeField] private Transform flashlightSocket;

    // Max distance from player before the flashlight teleports to the socket on drop
    [SerializeField] private float maxDropDistance = 3f;

    // Debug/testing: start flashlight enabled
    [System.Obsolete("Only for testing purposes.")]
    [SerializeField] private bool startEnabled = false;

    private TimerHandle batteryTimerHandle;

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

    // Set when flickering as part of turning off (e.g. dropped with low power)
    private bool forceOffAfterFlicker;

    // True when held by the player or socketed — suppresses the dropped-distance check
    private bool isSecured;

    // How much intensity one full crank rotation adds
    private const float LIGHT_MAGNITUDE = 10;

    // ==== Unity Lifecycle ====
    #region Unity Lifecycle
    /// <summary>
    /// Automatically fetch Light component if not set in inspector.
    /// </summary>
    private void Awake()
    {
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

        // Ensure flashlight starts off, timer created but immediately paused
        ToggleFlashLight(false);
        SetupActiveFlashlightTimer();
        TimerManager.Pause(this.batteryTimerHandle);

        if (startEnabled)
        {
            ToggleFlashLight(true);
            TimerManager.Resume(this.batteryTimerHandle);
        }
    }

    private void Update()
    {
        if (!isSecured)
            TeleportToSocketIfTooFar();

        if (!powered || lightSource == null) return;
        if (lightSource.intensity == targetLightIntensity && lightSource.range == targetLightRange) return;

        lightSource.intensity = Mathf.MoveTowards(lightSource.intensity, targetLightIntensity, 5f * Time.deltaTime);

        float normalized = Mathf.InverseLerp(flashlightSettings.GetMinLightPower(), flashlightSettings.GetMaxLightPower(), lightSource.intensity);
        targetLightRange = Mathf.Lerp(flashlightSettings.GetMinLightRange(), flashlightSettings.GetMaxLightRange(), normalized);
        lightSource.range = Mathf.MoveTowards(lightSource.range, targetLightRange, 5f * Time.deltaTime);

        flashlightSettings.CalculateDetectionCone(lightSource.range);
    }

    private void OnDestroy()
    {
        TimerManager.Release(ref this.batteryTimerHandle);
    }
    #endregion

    private void OnFlashlightDecay()
    {
        // Reduce target intensity
        UpdateLightIntensity(-this.flashlightSettings.GetLightDecayRate());

        // Turn off once target reaches minimum — check target, not the lerped source value
        if (this.targetLightIntensity <= this.flashlightSettings.GetMinLightPower())
        {
            Debug.Log("Flashlight reached minimum power, flickering out.");
            SetupFlashlightFlickerTimer();
        }
    }
    
    /// <summary>
    /// Adjusts target light intensity while clamping to limits.
    /// </summary>
    private void UpdateLightIntensity(float delta)
    {
        float clampedLightIntensity = Mathf.Clamp(this.targetLightIntensity + delta, this.flashlightSettings.GetMinLightPower(), this.flashlightSettings.GetMaxLightPower());
        this.targetLightIntensity = clampedLightIntensity;
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

        // Cranking restored power while the light was off — turn it on and start decay
        if (HasPower && !this.powered)
        {
            ToggleFlashLight(true);
            SetupActiveFlashlightTimer();
        }

    }

    // ==== Flashlight Helpers ====
    private void OnFlashlightDropped(SelectExitEventArgs args)
    {
        if (TeleportToSocketIfTooFar()) return; // isSecured stays true — now socketed
        isSecured = false;

        if (startEnabled) return;

        if (HasLowPower && HasPower)
        {
            this.forceOffAfterFlicker = true;
            SetupFlashlightFlickerTimer();
        }
        else
        {
            ToggleOffFlashlight();
        }
    }

    /// <summary>
    /// If the flashlight was dropped beyond maxDropDistance from the player,
    /// turns it off and snaps it to the player socket. Returns true if teleported.
    /// </summary>
    private bool TeleportToSocketIfTooFar()
    {
        if (this.playerTransform == null || this.flashlightSocket == null) return false;

        float dist = Vector3.Distance(this.transform.position, this.playerTransform.position);
        if (dist <= this.maxDropDistance) return false;

        ToggleOffFlashlight();
        this.transform.SetPositionAndRotation(this.flashlightSocket.position, this.flashlightSocket.rotation);
        return true;
    }

    /// <summary>
    /// Called when flashlight is grabbed. Only turns on if there is remaining power.
    /// </summary>
    private void OnFlashlightPickedup(SelectEnterEventArgs args)
    {
        isSecured = true;
        if (HasPower)
            ToggleOnFlashlight();
    }

    private void OnFlashlightFlicker()
    {
        flickeredLastFrame = !flickeredLastFrame;
        ToggleFlashLight(flickeredLastFrame);
    }
    
    /// <summary>
    /// Waits 6 seconds after pickup before the first decay tick fires.
    /// After the delay expires, switches to the repeating decay timer.
    /// </summary>
    private void SetupInitialDecayDelayTimer()
    {
        if (!TimerManager.Validate(this.batteryTimerHandle))
            this.batteryTimerHandle = TimerManager.Create(0, this.flashlightSettings.GetLightDecayTick());
        else
            TimerManager.Reconfigure(this.batteryTimerHandle, 0, this.flashlightSettings.GetLightDecayTick());

        TimerManager.SetCallbacks(this.batteryTimerHandle, null, SetupActiveFlashlightTimer);
    }

    private void SetupActiveFlashlightTimer()
    {
        if (!TimerManager.Validate(this.batteryTimerHandle))
            this.batteryTimerHandle = TimerManager.Create(this.flashlightSettings.GetLightDecayTick());
        else
            TimerManager.Reconfigure(this.batteryTimerHandle, this.flashlightSettings.GetLightDecayTick());

        TimerManager.SetCallbacks(this.batteryTimerHandle, OnFlashlightDecay, null);
    }

    private void SetupFlashlightFlickerTimer()
    {
        if (!TimerManager.Validate(this.batteryTimerHandle))
            this.batteryTimerHandle = TimerManager.Create(this.flashlightSettings.GetFlickerInterval(), this.flashlightSettings.GetFlickerTime());
        else
            TimerManager.Reconfigure(this.batteryTimerHandle, this.flashlightSettings.GetFlickerInterval(), this.flashlightSettings.GetFlickerTime());

        TimerManager.SetCallbacks(this.batteryTimerHandle, OnFlashlightFlicker, ResumeFlashlightAfterFlicker);
    }

    private void ResumeFlashlightAfterFlicker()
    {
        this.flickeredLastFrame = false;

        bool shouldTurnOn = HasPower && !this.forceOffAfterFlicker;
        this.forceOffAfterFlicker = false;

        if (shouldTurnOn)
        {
            ToggleFlashLight(true);
            SetupActiveFlashlightTimer();
        }
        else
        {
            ToggleFlashLight(false);
            TimerManager.Pause(this.batteryTimerHandle);
        }
    }
    
    /// <summary>
    /// Turns flashlight on when grabbed. Starts the 6-second initial delay before first decay.
    /// </summary>
    private void ToggleOnFlashlight()
    {
        ToggleFlashLight(true);
        SetupInitialDecayDelayTimer();
    }

    private void ToggleOffFlashlight()
    {
        ToggleFlashLight(false);
        TimerManager.Pause(this.batteryTimerHandle);
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

    // Whether flashlight has any remaining power (above minimum)
    public bool HasPower => this.targetLightIntensity > this.flashlightSettings.GetMinLightPower();

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
