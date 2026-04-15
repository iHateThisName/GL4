using System;
using UnityEngine;
using TMPro;
using UnityEngine.XR.Content.Interaction;

public class Radio : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private XRKnob knob;
    [SerializeField] private SO_RadioRef radioRef;
    [SerializeField] private TextMeshProUGUI channelText;

    [SerializeField] private AudioClip staticSound;
    [SerializeField] private AudioClip[] tips;
    [SerializeField] private AudioSource audioSource;

    [Header("Channel Settings")]
    [Tooltip("Number of discrete channels. Set the knob's Angle Increment to (maxAngle - minAngle) / (totalChannels - 1) to match visual snapping.")]
    [SerializeField] private int totalChannels = 9;
    [Tooltip("When true, forces the knob's SetValue each frame so Inspector slider changes fire onValueChange. Disable in production.")]
    [SerializeField] private bool editorTestMode = false;
    [Tooltip("The safe channel (1-indexed). Use 0 to auto-select the channel at angle 0.")]
    [SerializeField] private int safeChannel = 0;
    [Tooltip("Prefix added to channel for display (e.g., 88 shows channels as 89, 90, 91...)")]
    [SerializeField] private int channelPrefix = 88;
    [Tooltip("Channel to start on (1-indexed). Use 0 to start at the channel closest to angle 0.")]
    [SerializeField] private int startingChannel = 0;

    // Internal channel is 0-indexed
    private int currentChannelInternal = -1;
    // Resolved safe channel is 0-indexed
    private int resolvedSafeChannelInternal;
    private bool isOnSafeChannel;
    // Guards against callbacks arriving before Start() finishes
    private bool initialized;

    /// <summary>
    /// C# Action fired when channel changes. Parameters: (channel 1-indexed, isSafe)
    /// </summary>
    public event Action<int, bool> OnChannelChanged;

    private void Awake()
    {
        if (this.radioRef != null)
            this.radioRef.Value = this;
    }

    private void OnEnable()
    {
        if (knob != null)
            knob.onValueChange.AddListener(OnKnobValueChanged);

        GameManager.OnEventAvailable += OnNightEvent;
    }

    private void OnDisable()
    {
        if (knob != null)
            knob.onValueChange.RemoveListener(OnKnobValueChanged);

        GameManager.OnEventAvailable -= OnNightEvent;
    }

    private void Update()
    {
        if (!editorTestMode || !initialized || knob == null) return;
        knob.value = knob.value; // re-triggers SetValue → fires onValueChange
    }

    private void Start()
    {
        if (knob == null)
        {
            Debug.LogError("[Radio] No XRKnob assigned!", this);
            return;
        }

        // Resolve safe channel (0 means use center channel)
        if (safeChannel <= 0) resolvedSafeChannelInternal = StepAtAngleZero;
        else resolvedSafeChannelInternal = safeChannel - 1;

        resolvedSafeChannelInternal = Mathf.Clamp(resolvedSafeChannelInternal, 0, TotalChannels - 1);

        // Resolve starting channel (0 means use center channel)
        int resolvedStartingInternal;

        if (startingChannel <= 0) resolvedStartingInternal = StepAtAngleZero;
        else resolvedStartingInternal = startingChannel - 1;

        resolvedStartingInternal = Mathf.Clamp(resolvedStartingInternal, 0, TotalChannels - 1);

        // Set knob to starting channel — suppress event callbacks during init
        knob.value = StepToValue(resolvedStartingInternal);
        currentChannelInternal = ValueToStep(knob.value);
        isOnSafeChannel = IsOnChannel(SafeChannel);

        initialized = true;
        UpdateDebugUI();
    }

    private void OnNightEvent(NightEvent evt)
    {
        var eventData = evt.GetPayload();
        if (eventData.GetEventType() != NightEventType.RadioBroadcast) return;
        Debug.Log("Radio received broadcast event and is safe: " + isOnSafeChannel);

        if (!isOnSafeChannel) return;

        int tipIndex = UnityEngine.Random.Range(0, tips.Length);
        Debug.Log("Playing tip: " + tipIndex + " for " + tips[tipIndex].name + " with audiosource: " + this.audioSource);
        this.audioSource.clip = tips[tipIndex];
        this.audioSource.loop = false;
        this.audioSource.Play();
    }

    private void OnKnobValueChanged(float value)
    {
        if (!initialized) return;
        ApplyChannel(ValueToStep(value));
    }

    /// <summary>
    /// Sets the radio to a specific channel (1-indexed).
    /// </summary>
    public void SetChannel(int channel)
    {
        if (knob == null) return;

        int step = Mathf.Clamp(channel - 1, 0, TotalChannels - 1);
        knob.value = StepToValue(step);
    }

    private void ApplyChannel(int stepInternal)
    {
        if (stepInternal == currentChannelInternal) return;

        currentChannelInternal = stepInternal;
        isOnSafeChannel = IsOnChannel(SafeChannel);

        if (!isOnSafeChannel && staticSound != null)
        {
            this.audioSource.clip = this.staticSound;
            this.audioSource.loop = true;
            this.audioSource.Play();
        }
        else
        {
            Debug.Log("Radio stopped playing static sound");
            this.audioSource.Stop();
            this.audioSource.loop = false;
            this.audioSource.clip = null;
        }
        OnChannelChanged?.Invoke(CurrentChannel, isOnSafeChannel);
        UpdateDebugUI();
    }

    private void UpdateDebugUI()
    {
        if (channelText == null) return;
        channelText.text = $"CH: {CurrentFrequency}";
    }

    public bool IsOnChannel(int channel)
    {
        return CurrentChannel == channel;
    }

    private void OnValidate()
    {
        if (totalChannels < 2)
            totalChannels = 2;

        if (safeChannel > totalChannels) safeChannel = totalChannels;
        else if (safeChannel < 0) safeChannel = 0;

        if (startingChannel > totalChannels) startingChannel = totalChannels;
        else if (startingChannel < 0) startingChannel = 0;
    }

    #region Helpers
    private int ValueToStep(float value) => Mathf.RoundToInt(Mathf.Clamp01(value) * (TotalChannels - 1));
    private float StepToValue(int step) => TotalChannels > 1 ? step / (float)(TotalChannels - 1) : 0f;
    private int StepAtAngleZero => ValueToStep(Mathf.InverseLerp(knob.minAngle, knob.maxAngle, 0f));
    #endregion

    #region Getters
    /// <summary>Current channel (1-indexed, 1 to TotalChannels)</summary>
    public int CurrentChannel => currentChannelInternal + 1;

    /// <summary>Current channel with prefix (e.g., 89, 90, 91...)</summary>
    public int CurrentFrequency => channelPrefix + CurrentChannel;

    /// <summary>Total number of channels</summary>
    public int TotalChannels => totalChannels;

    /// <summary>The safe channel (1-indexed)</summary>
    public int SafeChannel => resolvedSafeChannelInternal + 1;

    /// <summary>The channel that corresponds to angle 0 on the knob (1-indexed)</summary>
    public int CenterChannel => knob != null ? StepAtAngleZero + 1 : 1;
    #endregion
}
