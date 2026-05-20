using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

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
    // Socket on the player where the flashlight snaps if dropped too far
    [SerializeField] private XRSocketInteractor flashlightSocket;
    
    [SerializeField] private string holsteredLayerName = "HolsteredItem";
    [SerializeField] private string crankInteractionLayer = "FlashlightHandle";

    // Max distance from player before the flashlight teleports to the socket on drop
    [SerializeField] private float maxDropDistance = 3f;

    // Debug/testing: start flashlight enabled
    [System.Obsolete("Only for testing purposes.")]
    [SerializeField] private bool startEnabled = false;

    private XRSimpleInteractable crankXRInteractable;
    private Transform playerTransform;
    
    private TimerHandle batteryTimerHandle;

    // Target intensity we smoothly move toward
    private float targetLightIntensity;

    // Target range (derived from intensity)
    private float targetLightRange;
    
    private int holsteredLayer;
    private int defaultLayer;
    private int crankLayerMask;

    // Whether flashlight is currently turned on
    private bool powered;
    
    // Whether the flashlight should flicker on or off this frame/flicker
    private bool flickeredLastFrame;

    // Set when flickering as part of turning off (e.g. dropped with low power)
    private bool forceOffAfterFlicker;

    // True when socketed or held — suppresses the dropped-distance check in Update
    private bool isSecured;

    // True only while physically held by the player
    private bool isHeld;

    private bool pickedUpOnce;

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
        
        this.defaultLayer = this.gameObject.layer;
        this.holsteredLayer = LayerMask.NameToLayer(holsteredLayerName);
        
        this.crankXRInteractable = this.crankInteractable.GetComponent<XRSimpleInteractable>();
        this.crankLayerMask = InteractionLayerMask.GetMask(this.crankInteractionLayer);
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

        if (this.flashlightSocket != null)
        {
            this.flashlightSocket.selectEntered.AddListener(OnFlashlightSocketed);
            this.flashlightSocket.selectExited.AddListener(OnFlashlightUnsocketed);
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

        if (this.flashlightSocket != null)
        {
            this.flashlightSocket.selectEntered.RemoveListener(OnFlashlightSocketed);
            this.flashlightSocket.selectExited.RemoveListener(OnFlashlightUnsocketed);
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
        this.flashlightSettings.CalculateDetectionCone(this.flashlightSettings.GetDetectionRangeForIntensity(this.targetLightIntensity));

        // Ensure flashlight starts off, timer created but immediately paused
        ToggleFlashLight(false);
        SetupActiveFlashlightTimer();
        TimerManager.Pause(this.batteryTimerHandle);

        if (this.startEnabled)
        {
            ToggleFlashLight(true);
            TimerManager.Resume(this.batteryTimerHandle);
        }

        this.playerTransform = Camera.main?.transform;
        this.pickedUpOnce = false;
    }

    private void Update()
    {
        if (!this.isHeld && !this.isSecured && this.pickedUpOnce)
            TeleportToSocketIfTooFar();

        if (!this.powered || this.lightSource == null) return;
        if (this.lightSource.intensity == this.targetLightIntensity && this.lightSource.range == this.targetLightRange) return;

        this.lightSource.intensity = Mathf.MoveTowards(this.lightSource.intensity, this.targetLightIntensity, 5f * Time.deltaTime);

        float normalized = Mathf.InverseLerp(this.flashlightSettings.GetMinLightPower(), this.flashlightSettings.GetMaxLightPower(), this.lightSource.intensity);
        this.targetLightRange = Mathf.Lerp(this.flashlightSettings.GetMinLightRange(), this.flashlightSettings.GetMaxLightRange(), normalized);
        this.lightSource.range = Mathf.MoveTowards(this.lightSource.range, this.targetLightRange, 5f * Time.deltaTime);

        this.flashlightSettings.CalculateDetectionCone(this.flashlightSettings.GetDetectionRangeForIntensity(this.lightSource.intensity));
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
        // Apply partial power based on rotation (gives immediate feedback while cranking)
        float partialPower = (Mathf.Abs(delta) / 360f) * LIGHT_MAGNITUDE;
        UpdateLightIntensity(partialPower);

        // Cranking restored power while the light was off — turn it on and start decay
        if (HasPower && !this.powered)
        {
            ToggleFlashLight(true);
            SetupActiveFlashlightTimer();
        }
    }
    
    /// <summary>
    /// If the flashlight was dropped beyond maxDropDistance from the player,
    /// turns it off and snaps it into the player socket via XR interaction. Returns true if teleported.
    /// </summary>
    private bool TeleportToSocketIfTooFar()
    {
        if (this.playerTransform == null || this.flashlightSocket == null) return false;

        float dist = Vector3.Distance(this.transform.position, this.playerTransform.position);
        if (dist <= this.maxDropDistance) return false;
        
        this.grabInteractable.interactionManager.SelectEnter((IXRSelectInteractor)this.flashlightSocket, this.grabInteractable);
        return true;
    }

    // ==== Flashlight Helpers ====
    private void OnFlashlightDropped(SelectExitEventArgs args)
    {
        isHeld = false;
        // Override whatever layer HandCollisionHandler restored (it may have saved the holstered layer).
        this.gameObject.layer = this.defaultLayer;
        
        if (this.crankXRInteractable != null)
            this.crankXRInteractable.interactionLayers &= ~crankLayerMask;

        if (TeleportToSocketIfTooFar()) return;

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
    /// Called when the flashlight is grabbed by the player's hand.
    /// </summary>
    private void OnFlashlightPickedup(SelectEnterEventArgs args)
    {
        if (args?.interactorObject is XRSocketInteractor) return;

        if (this.crankXRInteractable != null)
        {
            bool isLeftNearFar = args?.interactorObject is NearFarInteractor && args.interactorObject.handedness == InteractorHandedness.Left;
            if (isLeftNearFar)
                this.crankXRInteractable.interactionLayers |= crankLayerMask;
            else
                this.crankXRInteractable.interactionLayers &= ~crankLayerMask;
        }

        this.isHeld = true;
        if (this.HasPower)
            ToggleOnFlashlight();

        if (!this.pickedUpOnce) this.pickedUpOnce = true;
    }
    
    private void OnFlashlightSocketed(SelectEnterEventArgs args)
    {
        this.isSecured = true;
        this.isHeld = false;
        this.gameObject.layer = this.holsteredLayer;
        ToggleOffFlashlight();
    }

    // Fires when the socket releases the flashlight (player is picking it up).
    // Reset the layer to default now so HandCollisionHandler saves the right value.
    private void OnFlashlightUnsocketed(SelectExitEventArgs args)
    {
        this.isSecured = false;
    }

    private void OnFlashlightFlicker()
    {
        this.flickeredLastFrame = !this.flickeredLastFrame;
        ToggleFlashLight(this.flickeredLastFrame);
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

        bool shouldTurnOn = this.HasPower && !this.forceOffAfterFlicker && this.isHeld;
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
        TimerManager.Pause(this.batteryTimerHandle);
        this.flickeredLastFrame = false;
        this.forceOffAfterFlicker = false;
        ToggleFlashLight(false);
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
        OnFlashlightPickedup(null);
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
