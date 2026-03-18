using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class Radio : MonoBehaviour
{
    /* =======================
     * Serialized Fields
     * ======================= */

    [Header("Knob Reference")]
    [SerializeField] private ClampedKnob knob;

    [Header("Channel Settings")]
    [SerializeField] private int totalChannels = 5;
    [SerializeField] private int safeChannel = 2;
    [SerializeField] private int channelPrefix = 88;

    [Header("Debug UI")]
    [SerializeField] private TMP_Text debugChannelText;
    [SerializeField] private bool showDebugUI = true;

    [Header("Events")]
    public UnityEvent<int> OnChannelChanged;
    public UnityEvent OnTunedToSafeChannel;
    public UnityEvent OnTunedAwayFromSafeChannel;

    /* =======================
     * Private Fields
     * ======================= */

    private int currentChannel = -1;
    private bool wasOnSafeChannel;

    /* =======================
     * Properties
     * ======================= */

    /// <summary>Current channel index (0 to totalChannels-1)</summary>
    public int CurrentChannel => currentChannel;

    /// <summary>Current channel with prefix (e.g., 88, 89, 90...)</summary>
    public int CurrentFrequency => channelPrefix + currentChannel;

    /// <summary>Whether currently tuned to the safe channel</summary>
    public bool IsOnSafeChannel => currentChannel == safeChannel;

    /// <summary>Total number of channels</summary>
    public int TotalChannels => totalChannels;

    /// <summary>The safe channel index</summary>
    public int SafeChannel => safeChannel;

    /* =======================
     * Unity Lifecycle
     * ======================= */

    private void OnEnable()
    {
        if (knob != null)
        {
            knob.OnValueChanged.AddListener(OnKnobValueChanged);
        }
    }

    private void OnDisable()
    {
        if (knob != null)
        {
            knob.OnValueChanged.RemoveListener(OnKnobValueChanged);
        }
    }

    private void Start()
    {
        // Initialize channel from knob's current value
        if (knob != null)
        {
            UpdateChannelFromValue(knob.Value);
        }

        wasOnSafeChannel = IsOnSafeChannel;
        UpdateDebugUI();
    }

    /* =======================
     * Knob Callback
     * ======================= */

    private void OnKnobValueChanged(float value)
    {
        UpdateChannelFromValue(value);
    }

    private void UpdateChannelFromValue(float normalizedValue)
    {
        // Map normalized value (0-1) to channel index (0 to totalChannels-1)
        int newChannel = Mathf.FloorToInt(normalizedValue * totalChannels);

        // Handle edge case: when value = 1, we want the last channel
        newChannel = Mathf.Clamp(newChannel, 0, totalChannels - 1);

        if (newChannel == currentChannel) return;

        currentChannel = newChannel;

        // Fire channel changed event
        OnChannelChanged?.Invoke(currentChannel);

        // Check safe channel transitions
        bool isNowOnSafe = IsOnSafeChannel;

        if (isNowOnSafe && !wasOnSafeChannel)
        {
            OnTunedToSafeChannel?.Invoke();
        }
        else if (!isNowOnSafe && wasOnSafeChannel)
        {
            OnTunedAwayFromSafeChannel?.Invoke();
        }

        wasOnSafeChannel = isNowOnSafe;

        UpdateDebugUI();
    }

    /* =======================
     * Debug UI
     * ======================= */

    private void UpdateDebugUI()
    {
        if (debugChannelText == null || !showDebugUI) return;

        string safeIndicator = IsOnSafeChannel ? " [SAFE]" : "";
        debugChannelText.text = $"CH: {CurrentFrequency}{safeIndicator}";
    }

    /// <summary>
    /// Toggles debug UI visibility at runtime.
    /// </summary>
    public void SetDebugUIVisible(bool visible)
    {
        showDebugUI = visible;

        if (debugChannelText != null)
        {
            debugChannelText.gameObject.SetActive(visible);
        }

        if (visible)
        {
            UpdateDebugUI();
        }
    }

    /* =======================
     * Public API
     * ======================= */

    /// <summary>
    /// Sets the radio to a specific channel via the knob.
    /// </summary>
    public void SetChannel(int channel)
    {
        if (knob == null) return;

        channel = Mathf.Clamp(channel, 0, totalChannels - 1);

        // Calculate normalized value for the center of this channel
        float normalizedValue = (channel + 0.5f) / totalChannels;
        knob.SetValue(normalizedValue);
    }

    /// <summary>
    /// Sets the radio to the safe channel.
    /// </summary>
    public void TuneToSafeChannel()
    {
        SetChannel(safeChannel);
    }

    /// <summary>
    /// Checks if currently on a specific channel.
    /// </summary>
    public bool IsOnChannel(int channel)
    {
        return currentChannel == channel;
    }

    /// <summary>
    /// Checks if NOT on a specific channel.
    /// </summary>
    public bool IsNotOnChannel(int channel)
    {
        return currentChannel != channel;
    }

    /* =======================
     * Editor
     * ======================= */

    private void OnValidate()
    {
        if (totalChannels < 1)
        {
            totalChannels = 1;
        }

        safeChannel = Mathf.Clamp(safeChannel, 0, totalChannels - 1);
    }
}
