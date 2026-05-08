using TMPro;
using UnityEngine;

/// <summary>
/// In-game wrist watch HUD that displays the player's current hunger level,
/// body temperature state, and the in-world time of night.
/// Subscribes to <see cref="PlayerTemperatureSimulator"/> and <see cref="HungerSystem"/> events
/// so all three displays stay in sync without polling.
/// </summary>
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

    /// <summary>
    /// Computes and caches the total usable night duration for the clock display,
    /// accounting for the fixed 8 AM end offset.
    /// </summary>
    private void Start()
    {
        this.totalDuration = this.nightSettings.GetNightTimeInSeconds() - this.timeAt8AM;
    }

    /// <summary>
    /// Subscribes to temperature and hunger events so the HUD reacts to game-state changes.
    /// </summary>
    private void OnEnable()
    {
        PlayerTemperatureSimulator.OnBodyTemperatureStateChanged += HandleTemperatureChanged;

        HungerSystem.OnHungerChanged += OnHungerChanged;
        HungerSystem.HungerStateChangedEvent += OnHungerStateChanged;
    }

    /// <summary>
    /// Unsubscribes from all events to prevent stale callbacks after the watch is disabled.
    /// </summary>
    private void OnDisable()
    {
        PlayerTemperatureSimulator.OnBodyTemperatureStateChanged -= HandleTemperatureChanged;

        HungerSystem.OnHungerChanged -= OnHungerChanged;
        HungerSystem.HungerStateChangedEvent -= OnHungerStateChanged;
    }

    /// <summary>
    /// Called every frame. Keeps the time display current.
    /// </summary>
    private void Update()
    {
        UpdateTimeUI();
    }

    /// <summary>
    /// Handles a body temperature state change by updating the temperature label text
    /// and its display color to match the new state.
    /// </summary>
    /// <param name="change">Struct containing the previous and current temperature states.</param>
    private void HandleTemperatureChanged(BodyTemperatureStateChange change)
    {
        this.temperatureText.text = change.CurrentState.ToString();
        this.temperatureText.color = GetTemperatureColor(change.CurrentState);
    }

    /// <summary>
    /// Maps a body temperature state to its configured display color from
    /// <see cref="temperatureStateColors"/>. Returns white as a fallback.
    /// </summary>
    /// <param name="state">The body temperature state to look up.</param>
    /// <returns>The color associated with that state.</returns>
    private Color GetTemperatureColor(PlayerTemperatureSimulator.EnumBodyTemperatureState state)
    {
        return state switch
        {
            PlayerTemperatureSimulator.EnumBodyTemperatureState.Normal => temperatureStateColors[3],
            PlayerTemperatureSimulator.EnumBodyTemperatureState.MildHypothermia => temperatureStateColors[2],
            PlayerTemperatureSimulator.EnumBodyTemperatureState.ModerateHypothermia => temperatureStateColors[1],
            PlayerTemperatureSimulator.EnumBodyTemperatureState.Hypothermia => temperatureStateColors[0],
            PlayerTemperatureSimulator.EnumBodyTemperatureState.MildHyperthermia => temperatureStateColors[4],
            PlayerTemperatureSimulator.EnumBodyTemperatureState.ModerateHyperthermia => temperatureStateColors[5],
            PlayerTemperatureSimulator.EnumBodyTemperatureState.Hyperthermia => temperatureStateColors[6],
            _ => Color.white
        };
    }
    
    /// <summary>
    /// Called when the player's raw hunger value changes. Refreshes the hunger display.
    /// </summary>
    /// <param name="hunger">The new hunger value.</param>
    private void OnHungerChanged(float hunger)
    {
        Debug.Log("Hunger Changed: " + hunger);
        UpdateHungerUI();
    }

    /// <summary>
    /// Called when the player's hunger state changes (e.g., from Full to Hungry).
    /// Refreshes the hunger display to reflect both the new state and current value.
    /// </summary>
    /// <param name="previous">The hunger state before the change.</param>
    /// <param name="current">The hunger state after the change.</param>
    private void OnHungerStateChanged(HungerSystem.EnumHungerState previous, HungerSystem.EnumHungerState current)
    {
        Debug.Log("Hunger State Changed: " + previous + " -> " + current);
        UpdateHungerUI();
    }

    /// <summary>
    /// Writes the current hunger state and percentage to the hunger text label.
    /// </summary>
    private void UpdateHungerUI()
    {
        if (this.hungerText != null)
            this.hungerText.text = "(" + this.hungerSystem.State + ") " + this.hungerSystem.Hunger.ToString("F0") + "%";
    }

    /// <summary>
    /// Reads the current night time from <see cref="GameManager"/> and writes it
    /// to the time text label.
    /// </summary>
    private void UpdateTimeUI()
    {
        if (this.timeText != null)
        {
            float current = GameManager.Instance.NightTime;
            this.timeText.text = GetNightTime(current);
        }
    }

    /// <summary>
    /// Converts a raw night-time float (seconds elapsed since midnight) into an AM
    /// clock string (e.g., "3 AM"). The night spans 12 AM through 8 AM in eight equal segments.
    /// </summary>
    /// <param name="current">Seconds elapsed since the start of the night.</param>
    /// <returns>A string of the form "H AM" representing the current in-game hour.</returns>
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