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
        UpdateColor(change.CurrentState);
    }

    private void UpdateColor(PlayerTemperatureSimulator.EnumBodyTemperatureState state) 
    {
        switch (state) 
        {
            case PlayerTemperatureSimulator.EnumBodyTemperatureState.ModerateHypothermia:
                this.temperatureText.color = Color.blue;
                break;
            case PlayerTemperatureSimulator.EnumBodyTemperatureState.MildHypothermia:
                this.temperatureText.color = Color.cyan;
                break;
            case PlayerTemperatureSimulator.EnumBodyTemperatureState.Normal:
                this.temperatureText.color = Color.green;
                break;
            case PlayerTemperatureSimulator.EnumBodyTemperatureState.MildHyperthermia:
                this.temperatureText.color = Color.yellow;
                break;
            case PlayerTemperatureSimulator.EnumBodyTemperatureState.ModerateHyperthermia:
                this.temperatureText.color = Color.red;
                break;
            case PlayerTemperatureSimulator.EnumBodyTemperatureState.Hyperthermia:
            case PlayerTemperatureSimulator.EnumBodyTemperatureState.Hypothermia:
                this.temperatureText.color = Color.magenta;
                break;
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

        return hour.ToString() + " AM";
    }
}