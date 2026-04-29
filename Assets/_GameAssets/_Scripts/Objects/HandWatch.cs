using System;
using TMPro;
using UnityEngine;

public class HandWatch : MonoBehaviour
{
    [Header("====== References ======")]
    [SerializeField] private HungerSystem hungerSystem;
    [SerializeField] private SO_NightSettings nightSettings;
    
    [Header("====== UI References ======")]
    [SerializeField] private TextMeshProUGUI hungerText;
    [SerializeField] private TextMeshProUGUI temperatureText;
    [SerializeField] private TextMeshProUGUI timeText;
    
    [Header("==== Night Settings ====")]
    [SerializeField] private float timeAt8AM = 5f;
    [Tooltip("Colors for each temperature state. 0/top most is Coldest")]
    [SerializeField] private Color[] temperatureStateColors = new Color[7]
    {
        Color.magenta, // Hypothermia
        Color.blue, // Moderate Hypothermia
        Color.cyan, // Mild Hypothermia
        Color.green, // Normal
        Color.yellow, // Mild Hyperthermia
        Color.red, // Moderate Hyperthermia
        Color.white // Hyperthermia
    };
    
    private float totalDuration;

    private void Start()
    {
        this.totalDuration = this.nightSettings.GetNightTimeInSeconds() - this.timeAt8AM;
    }

    private void OnEnable()
    {
        PlayerTemperatureSimulator.OnBodyTemperatureStateChanged += HandleTemperatureChanged;
        
        HungerSystem.OnHungerChanged += OnHungerChanged;
        HungerSystem.HungerStateChangedEvent += OnHungerStateChanged;
    }
    
    private void OnDisable() 
    {
        PlayerTemperatureSimulator.OnBodyTemperatureStateChanged -= HandleTemperatureChanged;
        
        HungerSystem.OnHungerChanged -= OnHungerChanged;
        HungerSystem.HungerStateChangedEvent -= OnHungerStateChanged;
    }

    private void Update()
    {
        UpdateTimeUI();
    }
    
    private void HandleTemperatureChanged(BodyTemperatureStateChange change) 
    {
        this.temperatureText.text = change.CurrentState.ToString();
        this.temperatureText.color = GetTemperatureColor(change.CurrentState);
    }

    private Color GetTemperatureColor(PlayerTemperatureSimulator.EnumBodyTemperatureState state)
    {
        switch (state)
        {
            case PlayerTemperatureSimulator.EnumBodyTemperatureState.Normal:
                return temperatureStateColors[3];
                break;
            case PlayerTemperatureSimulator.EnumBodyTemperatureState.MildHypothermia:
                return temperatureStateColors[2];
                break;
            case PlayerTemperatureSimulator.EnumBodyTemperatureState.ModerateHypothermia:
                return temperatureStateColors[1];
                break;
            case PlayerTemperatureSimulator.EnumBodyTemperatureState.Hypothermia:
                return temperatureStateColors[0];
                break;
            case PlayerTemperatureSimulator.EnumBodyTemperatureState.MildHyperthermia:
                return temperatureStateColors[4];
                break;
            case PlayerTemperatureSimulator.EnumBodyTemperatureState.ModerateHyperthermia:
                return temperatureStateColors[5];
                break;
            case PlayerTemperatureSimulator.EnumBodyTemperatureState.Hyperthermia:
                return temperatureStateColors[6];
                break;
            default:
                return Color.white;
        }
    }
    
    private void OnHungerChanged(float hunger)
    {
        Debug.Log("Hunger Changed: " + hunger);
        UpdateHungerUI();
    }
    
    private void OnHungerStateChanged(HungerSystem.EnumHungerState previous, HungerSystem.EnumHungerState current)
    {
        Debug.Log("Hunger State Changed: " + previous + " -> " + current);
        UpdateHungerUI();
    }
    
    private void UpdateHungerUI()
    {
        if (this.hungerText != null) 
            this.hungerText.text = "(" + this.hungerSystem.State + ") " + this.hungerSystem.Hunger.ToString("F0") + "%";
    }
    
    private void UpdateTimeUI()
    {
        if (this.timeText != null)
        {
            float current = GameManager.Instance.NightTime;
            this.timeText.text = GetNightTime(current);
        }
    }

    private string GetNightTime(float current)
    {
        int hour;
        if (current >= this.totalDuration)
        {
            hour = 8;
        }
        else
        {
            float segment = totalDuration / 8f; // 12 through 7 = 8 segments
            int index = Mathf.FloorToInt(current / segment);

            hour = index == 0 ? 12 : index;
        }

        return hour + " AM";
    }
}