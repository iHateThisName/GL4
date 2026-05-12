using UnityEngine;

public class BlizzardController : MonoBehaviour
{
    [Tooltip("The VFX GameObject to turn on when cold.")]
    [SerializeField] private GameObject coldVFX;

    private void OnEnable()
    {
        // Subscribe to the location change event
        PlayerTemperatureSimulator.OnLocationTypeChanged += HandleLocationTypeChanged;
    }

    private void OnDisable()
    {
        // Always unsubscribe to prevent memory leaks
        PlayerTemperatureSimulator.OnLocationTypeChanged -= HandleLocationTypeChanged;
    }

    private void Start()
    {
        // Set the initial state of the VFX when the scene loads
        if (PlayerTemperatureSimulator.Instance != null)
        {
            HandleLocationTypeChanged(PlayerTemperatureSimulator.Instance.CurrentLocationType);
        }
    }

    /// <summary>
    /// Called whenever the player's location type changes.
    /// </summary>
    /// <param name="newLocation">The new location type.</param>
    private void HandleLocationTypeChanged(PlayerTemperatureSimulator.EnumLocationType newLocation)
    {
        if (coldVFX == null) return;

        // Turn the VFX on ONLY if the location is Cold. 
        // Normal, Warm, Shack, etc., will turn it off.
        bool isCold = (newLocation == PlayerTemperatureSimulator.EnumLocationType.Cold);
        coldVFX.SetActive(isCold);
    }

    //[SerializeField] private ParticleSystem blizzardParticleSystem;
    //[SerializeField] private SO_NightSettings nightSettings;

    //[Header("Blizzard Intensity Settings")]
    //[SerializeField] private float minEmissionRate = 10f;
    //[SerializeField] private float maxEmissionRate = 100f;
    //[SerializeField] private float minParticleSpeed = 5f;
    //[SerializeField] private float maxParticleSpeed = 20f;
    //[SerializeField] private float minParticleSize = 0.1f;
    //[SerializeField] private float maxParticleSize = 0.5f;

    //private ParticleSystem.EmissionModule emissionModule;
    //private ParticleSystem.VelocityOverLifetimeModule velocityModule;
    //private ParticleSystem.SizeOverLifetimeModule sizeModule;

    //void Start()
    //{
    //    if (blizzardParticleSystem == null)
    //    {
    //        blizzardParticleSystem = GetComponent<ParticleSystem>();
    //    }

    //    if (blizzardParticleSystem != null)
    //    {
    //        emissionModule = blizzardParticleSystem.emission;
    //        velocityModule = blizzardParticleSystem.velocityOverLifetime;
    //        sizeModule = blizzardParticleSystem.sizeOverLifetime;
    //    }
    //}

    //void Update()
    //{
    //    UpdateBlizzardIntensity();
    //}

    //private void UpdateBlizzardIntensity()
    //{
    //    if (blizzardParticleSystem == null || GameManager.Instance == null || nightSettings == null)
    //        return;

    //    // Calculate night progression (0 to 1, where 1 is end of night)
    //    float nightTime = GameManager.Instance.NightTime;
    //    float totalNightTime = nightSettings.GetNightTimeInSeconds();
    //    float nightProgress = Mathf.Clamp01(totalNightTime > 0 ? nightTime / totalNightTime : 0f);

    //    // Apply intensity scaling to particle system
    //    UpdateEmissionRate(nightProgress);
    //    UpdateParticleSpeed(nightProgress);
    //    UpdateParticleSize(nightProgress);
    //}

    //private void UpdateEmissionRate(float nightProgress)
    //{
    //    // Use exponential growth for faster particle emission increase
    //    float growthFactor = Mathf.Pow(nightProgress, 0.3f); // Lower exponent = faster growth
    //    float newEmissionRate = Mathf.Lerp(minEmissionRate, maxEmissionRate, growthFactor);
    //    emissionModule.rateOverTime = newEmissionRate;
    //}

    //private void UpdateParticleSpeed(float nightProgress)
    //{
    //    float newSpeed = Mathf.Lerp(minParticleSpeed, maxParticleSpeed, nightProgress);

    //    // Update velocity over lifetime for speed variation
    //    var vel = velocityModule;
    //    vel.xMultiplier = newSpeed * 0.5f;
    //    vel.yMultiplier = newSpeed * 0.5f;
    //    vel.zMultiplier = newSpeed;
    //    velocityModule = vel;
    //}

    //private void UpdateParticleSize(float nightProgress)
    //{
    //    // Use exponential growth for faster size increase
    //    float growthFactor = Mathf.Pow(nightProgress, 0.1f); // Lower exponent = faster growth
    //    float newSize = Mathf.Lerp(minParticleSize, maxParticleSize, growthFactor);

    //    var size = sizeModule;
    //    size.separateAxes = false;
    //    size.z = newSize;
    //    sizeModule = size;
    //}
}
