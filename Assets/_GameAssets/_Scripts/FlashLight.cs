using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))] // Ensures the object always has an XRGrabInteractable component
public class FlashLight : MonoBehaviour
{
    [Header("=== References ===")]
    [SerializeField] private XRGrabInteractable handleInteractable;
    [SerializeField] private CrankRotationInteractable crankInteractable;
    [SerializeField] private Light light;
    [Header("=== Light Settings ===")]
    [SerializeField] private float startingLightPower = 40f;
    [SerializeField] private float minLightPower = 3f;
    [SerializeField] private float maxLightPower = 140f;
    [SerializeField] private float startingLightRange = 6f;
    [SerializeField] private float maxLightRange = 12f;
    [SerializeField] private float minLightRange = 4f;
    [SerializeField] private float lightDecayRate = 1.5f;
    [SerializeField] private float lightDecayTickMax = 15f;
    [SerializeField] private float lightDecayTickMin = 3f;
    [System.Obsolete("Only for testing purposes.")]
    [SerializeField] private bool startEnabled = false;
    [Header("=== Power Settings ===")]
    [SerializeField] private float lowPowerThreshold = 5f;
    [SerializeField] private int maxRotations = 10;
    
    public float currentAngle { get; private set; }
    public int fullRotations { get; private set; }
    
    private float elapsedTime;
    private float currentLightDecayTick;
    private float targetLightIntensity;
    private float targetLightRange;
    public bool poweredOn = false;
    
    private const float LIGHT_MAGNITUDE = 10;
    
    //  ==== Unity Lifecycle ==== \\
    #region Unity Lifecycle
    private void OnEnable()
    {
        if (crankInteractable != null) 
            crankInteractable.OnCrank += OnCrankRotated;
        
        if (handleInteractable != null)
        {
            handleInteractable.selectEntered.AddListener(ToggleOnFlashlight);
            handleInteractable.selectExited.AddListener(ToggleOffFlashlight);
        }
    }

    private void OnDisable()
    {
        if (crankInteractable != null) 
            crankInteractable.OnCrank -= OnCrankRotated;

        if (handleInteractable != null)
        {
            handleInteractable.selectEntered.RemoveListener(ToggleOnFlashlight);
            handleInteractable.selectExited.RemoveListener(ToggleOffFlashlight);
        }
    }
    
    private void Start()
    {
        this.elapsedTime = 0f;
        this.currentLightDecayTick = this.lightDecayTickMin;
        RandomizeLightDecayTick();
        this.poweredOn = false;
        
        if (this.light != null)
        {
            this.light.intensity = this.startingLightPower;
            this.light.range = this.startingLightRange;
            this.targetLightIntensity = this.startingLightPower;
        }
        ToggleFlashLight(false);
        
        if (startEnabled) ToggleFlashLight(true);
    }

    private void Update()
    {
        if (!this.poweredOn) return;
        
        this.elapsedTime += Time.deltaTime;
        
        UpdateFlashLight();

        // Only decay hunger once the tick interval has passed
        if (this.elapsedTime >= this.currentLightDecayTick)
        {
            this.elapsedTime = 0f;
            RandomizeLightDecayTick();
            
            if (this.LightIntensity <= minLightPower)
            {
                ToggleFlashLight(false);
                return;
            }
            
            UpdateLightIntensity(-this.lightDecayRate);
        }
    }
    
    #if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying) return;
        UpdateLightIntensity(this.startingLightPower);
    }
    #endif
    #endregion
    
    public bool HasLowPower => this.LightIntensity <= this.lowPowerThreshold;
    
    //  ==== Crank ==== \\
    void OnCrankRotated(float delta)
    {
        if (this.fullRotations >= this.maxRotations) return;
        
        this.currentAngle += delta;
        
        // Count full rotations
        while (this.currentAngle >= 360f)
        {
            this.currentAngle -= 360f;
            this.fullRotations++;
            UpdateLightIntensity(LIGHT_MAGNITUDE);
        }

        while (this.currentAngle <= -360f)
        {
            this.currentAngle += 360f;
            this.fullRotations--;
            UpdateLightIntensity(-LIGHT_MAGNITUDE);
        }
    }

    //  ==== Light ==== \\
    public float LightIntensity => this.light != null ? this.light.intensity : this.startingLightPower;
    
    private void RandomizeLightDecayTick()
    {
        this.currentLightDecayTick = Random.Range(this.lightDecayTickMin, this.lightDecayTickMax);
    }
    
    private void UpdateLightIntensity(float newLightValue)
    {
        float clampedLightIntensity = Mathf.Clamp(this.targetLightIntensity + newLightValue, minLightPower, this.maxLightPower);
        this.targetLightIntensity = clampedLightIntensity;
    }

    private void UpdateFlashLight()
    {
        if (this.light == null) return;
        
        this.light.intensity = Mathf.MoveTowards(this.LightIntensity, targetLightIntensity, 5f * Time.deltaTime);
        
        float normalized = Mathf.InverseLerp(this.minLightPower, this.maxLightPower, this.LightIntensity);
        this.light.range = Mathf.Lerp(this.minLightRange, this.maxLightRange, normalized);
    }
    
    // ==== Flashlight Helpers ==== \\
    private void ToggleOnFlashlight(SelectEnterEventArgs args)
    {
        ToggleFlashLight(true);
    }

    private void ToggleOffFlashlight(SelectExitEventArgs args)
    {
        ToggleFlashLight(false);
    }
    
    private void ToggleFlashLight(bool toggle)
    {
        this.poweredOn = toggle;
        this.light.enabled = this.poweredOn;
    }
}
