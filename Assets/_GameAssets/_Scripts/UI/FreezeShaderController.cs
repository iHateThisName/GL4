using UnityEngine;
using UnityEngine.UI;

public class FreezeShaderController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image freezeImage;

    [Header("Shader Settings")]
    [Tooltip("The exact Reference name of the property in your Shader Graph")]
    [SerializeField] private string shaderPropertyName = "_VignettePower"; // Changed to Power

    [Header("Effect Tuning")]
    [Tooltip("The shader power value when the player is warm (Effect is starting to fade in)")]
    [SerializeField] private float powerAtNormal = 10f;

    [Tooltip("The shader power value when the player is freezing to death (Effect is maximum)")]
    [SerializeField] private float powerAtDeath = 2f;

    // Temperature Thresholds (Matching your PlayerTemperatureSimulator)
    private const float NORMAL_TEMP_THRESHOLD = 35.2f;
    private const float FREEZING_DEATH_TEMP = 28.0f;

    private Material instancedMaterial;
    private int shaderPropertyID;

    private void Start()
    {
        if (freezeImage != null && freezeImage.material != null)
        {
            // Clone the material so we don't overwrite the project asset
            instancedMaterial = new Material(freezeImage.material);
            freezeImage.material = instancedMaterial;

            // Cache the property ID
            shaderPropertyID = Shader.PropertyToID(shaderPropertyName);
        }
    }

    private void FixedUpdate()
    {
        if (PlayerTemperatureSimulator.Instance == null || freezeImage == null || instancedMaterial == null) return;

        float currentTemp = PlayerTemperatureSimulator.Instance.CurrentBodyTemperature;

        // If warm, disable the image completely
        if (currentTemp >= NORMAL_TEMP_THRESHOLD)
        {
            if (freezeImage.enabled)
            {
                freezeImage.enabled = false;
            }
            return;
        }

        // If cold, enable the image
        if (!freezeImage.enabled)
        {
            freezeImage.enabled = true;
        }

        // Calculate how close we are to freezing (0.0 to 1.0)
        float temperaturePercentage = Mathf.InverseLerp(FREEZING_DEATH_TEMP, NORMAL_TEMP_THRESHOLD, currentTemp);

        // Map the temperature percentage to your new Power values
        float currentPower = Mathf.Lerp(powerAtDeath, powerAtNormal, temperaturePercentage);

        // Apply to the shader
        instancedMaterial.SetFloat(shaderPropertyID, currentPower);
    }

    private void OnDestroy()
    {
        if (instancedMaterial != null)
        {
            Destroy(instancedMaterial);
        }
    }
}