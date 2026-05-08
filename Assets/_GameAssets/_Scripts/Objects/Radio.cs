using System;
using UnityEngine;
using TMPro;
using UnityEngine.XR.Content.Interaction;
using Random = UnityEngine.Random;

public class Radio : MonoBehaviour
{
    public enum RadioBroadcasts
    {
        Static = 0,
        IntroductionTutorial = 1,
        FoodTutorialTip = 2,
        FireTutorialTip = 3,
        RadioTutorialTip = 4,
        NightTwoWeather = 5,
        LostConnectionWithOutside = 6,
        LastBroadcast = 7,
        Off = 8,
    }

    [Header("References")] [SerializeField]
    private XRKnob knob;

    [SerializeField] private SO_RadioRef radioRef;
    [SerializeField] private TextMeshProUGUI channelText;

    [Header("Channel Settings")]
    [Tooltip("Number of discrete channels. Set the knob's Angle Increment to (maxAngle - minAngle) / (totalChannels - 1) to match visual snapping.")]
    [SerializeField]
    private int totalChannels = 9;

    [Tooltip("When true, forces the knob's SetValue each frame so Inspector slider changes fire onValueChange. Disable in production.")]
    [SerializeField]
    private bool editorTestMode = false;

    [Tooltip("The safe channel (1-indexed). Use 0 to auto-select the channel at angle 0.")] [SerializeField]
    private int safeChannel = 0;

    [Tooltip("Prefix added to channel for display (e.g., 88 shows channels as 89, 90, 91...)")] [SerializeField]
    private int channelPrefix = 88;

    [Tooltip("Channel to start on (1-indexed). Use 0 to start at the channel closest to angle 0.")] [SerializeField]
    private int startingChannel = 0;

    private FMODUnity.StudioEventEmitter fmodEmitter;
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

    /// <summary>
    /// C# Action fired when a broadcast starts playing.
    /// </summary>
    public event Action<RadioBroadcasts> OnBroadcastChanged;
    
    private const string RADIO_PARAMATER_NAME = "RadioHost";

    private void Awake()
    {
        if (this.radioRef != null)
            this.radioRef.Value = this;
        
        this.fmodEmitter = GetComponent<FMODUnity.StudioEventEmitter>();
    }

    private void OnEnable()
    {
        if (this.knob != null)
            this.knob.onValueChange.AddListener(OnKnobValueChanged);

        GameManager.OnEventAvailable += OnNightEvent;
    }

    private void OnDisable()
    {
        if (this.knob != null)
            this.knob.onValueChange.RemoveListener(OnKnobValueChanged);

        GameManager.OnEventAvailable -= OnNightEvent;
    }

    private void Start()
    {
        if (this.knob == null)
        {
            Debug.LogError("[Radio] No XRKnob assigned!", this);
            return;
        }

        // Resolve safe channel (0 means use center channel)
        if (this.safeChannel <= 0) this.resolvedSafeChannelInternal = this.StepAtAngleZero;
        else this.resolvedSafeChannelInternal = this.safeChannel - 1;

        this.resolvedSafeChannelInternal = Mathf.Clamp(this.resolvedSafeChannelInternal, 0, this.TotalChannels - 1);

        // Resolve starting channel (0 means use center channel)
        int resolvedStartingInternal;

        if (this.startingChannel <= 0) resolvedStartingInternal = this.StepAtAngleZero;
        else resolvedStartingInternal = this.startingChannel - 1;

        resolvedStartingInternal = Mathf.Clamp(resolvedStartingInternal, 0, this.TotalChannels - 1);
        
        // Set knob to starting channel — suppress event callbacks during init
        this.knob.value = StepToValue(resolvedStartingInternal);
        this.currentChannelInternal = ValueToStep(this.knob.value);
        this.isOnSafeChannel = IsOnChannel(this.SafeChannel);

        this.initialized = true;
        UpdateDebugUI();
    }

    private void OnNightEvent(NightEvent evt)
    {
        if (evt.GetEventType() != NightEvent.NightEventType.RadioBroadcast) return;
        PlayBroadcast(evt.GetRadioBroadcast());
    }

    private void PlayBroadcast(RadioBroadcasts broadcast)
    {
        if (!this.fmodEmitter.EventInstance.hasHandle())
            this.fmodEmitter.Play();
        this.fmodEmitter.SetParameter(RADIO_PARAMATER_NAME, (int)broadcast);
        OnBroadcastChanged?.Invoke(broadcast);
        Debug.Log($"Playing {broadcast} on radio");
    }

    public void SendBroadcast(RadioBroadcasts broadcast) => PlayBroadcast(broadcast);

    private void OnKnobValueChanged(float value)
    {
        if (!this.initialized) return;
        ApplyChannel(ValueToStep(value));
    }

/// <summary>
    /// Sets the radio to a specific channel (1-indexed).
    /// </summary>
    public void SetChannel(int channel)
    {
        if (this.knob == null) return;

        int step = Mathf.Clamp(channel - 1, 0, this.TotalChannels - 1);
        this.knob.value = StepToValue(step);
    }

    private void ApplyChannel(int stepInternal)
    {
        if (stepInternal == this.currentChannelInternal) return;

        this.currentChannelInternal = stepInternal;
        this.isOnSafeChannel = IsOnChannel(this.SafeChannel);

        if (!this.isOnSafeChannel) PlayBroadcast(RadioBroadcasts.Static);
        else PlayBroadcast(RadioBroadcasts.Off);
        
        OnChannelChanged?.Invoke(this.CurrentChannel, this.isOnSafeChannel);
        UpdateDebugUI();
    }

    [ContextMenu("Test Broadcast")]
    private void TestBroadcast()
    {
        PlayBroadcast(RadioBroadcasts.LastBroadcast);
    }

    private void UpdateDebugUI()
    {
        if (this.channelText == null) return;
        this.channelText.text = $"CH: {this.DisplayedChannel}";
    }

    public bool IsOnChannel(int channel)
    {
        return this.CurrentChannel == channel;
    }

    [ContextMenu("Set Random Channel")]
    private void ContextSetRandomChannel()
    {
        int channel;
        do { channel = Random.Range(1, this.TotalChannels + 1); }
        while (channel == this.SafeChannel && this.TotalChannels > 1);
        SetChannel(channel);
    }

    [ContextMenu("Set Safe Channel")]
    private void ContextSetSafeChannel()
    {
        SetChannel(this.SafeChannel);
    }

    private void OnValidate()
    {
        if (this.totalChannels < 2)
            this.totalChannels = 2;

        if (this.safeChannel > this.totalChannels) this.safeChannel = this.totalChannels;
        else if (this.safeChannel < 0) this.safeChannel = 0;

        if (this.startingChannel > this.totalChannels) this.startingChannel = this.totalChannels;
        else if (this.startingChannel < 0) this.startingChannel = 0;
    }

    #region Helpers
    private int ValueToStep(float value) => Mathf.RoundToInt(Mathf.Clamp01(value) * (this.TotalChannels - 1));
    private float StepToValue(int step) => this.TotalChannels > 1 ? step / (float)(this.TotalChannels - 1) : 0f;
    private int StepAtAngleZero => ValueToStep(Mathf.InverseLerp(this.knob.minAngle, this.knob.maxAngle, 0f));
    #endregion

    #region Getters
    /// <summary>Current channel (1-indexed, 1 to TotalChannels)</summary>
    public int CurrentChannel => this.currentChannelInternal + 1;

    /// <summary>Current channel with prefix (e.g., 89, 90, 91...)</summary>
    public int DisplayedChannel => this.channelPrefix + this.CurrentChannel;

    /// <summary>Total number of channels</summary>
    public int TotalChannels => this.totalChannels;

    /// <summary>The safe channel (1-indexed)</summary>
    public int SafeChannel => this.resolvedSafeChannelInternal + 1;

    /// <summary>The channel that corresponds to angle 0 on the knob (1-indexed)</summary>
    public int CenterChannel => knob != null ? this.StepAtAngleZero + 1 : 1;
    #endregion
}
