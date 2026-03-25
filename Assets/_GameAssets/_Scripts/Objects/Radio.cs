using System;
using UnityEngine;
using TMPro;

public class Radio : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ClampedKnob knob;
    [SerializeField] private SO_RadioRef radioRef;
    [SerializeField] private TextMeshProUGUI channelText;

    [Header("Channel Settings")]
    [Tooltip("If true, Radio sets the knob's steps to match totalChannels. If false, uses the knob's existing steps.")]
    [SerializeField] private bool overrideKnobSteps = true;
    [Tooltip("Number of channels. Only used if overrideKnobSteps is true.")]
    [SerializeField] private int totalChannels = 9;
    [Tooltip("The safe channel (1-indexed). Use 0 to auto-select the channel at angle 0.")]
    [SerializeField] private int safeChannel = 0;
    [Tooltip("Prefix added to channel for display (e.g., 88 shows channels as 89, 90, 91...)")]
    [SerializeField] private int channelPrefix = 88;
    [Tooltip("Channel to start on (1-indexed). Use 0 to start at the channel closest to angle 0.")]
    [SerializeField] private int startingChannel = 0;

    // Internal channel is 0-indexed (matches knob.CurrentStep)
    private int currentChannelInternal = -1;
    // Resolved safe channel is 0-indexed
    private int resolvedSafeChannelInternal;
    // Whether currently tuned to the safe channel</summary>
    private bool isOnSafeChannel;
    
    /// <summary>
    /// C# Action fired when channel changes. Parameters: (channel 1-indexed, isSafe)
    /// </summary>
    public event Action<int, bool> OnChannelChanged;
    
    private void Awake()
    {
        if (this.radioRef != null)
            this.radioRef.Value = this;

        // Set knob steps before anything else initializes
        if (knob != null && overrideKnobSteps)
            knob.Steps = totalChannels;
    }

    private void OnEnable()
    {
        if (knob != null)
            knob.OnStepChanged.AddListener(OnKnobStepChanged);
    }

    private void OnDisable()
    {
        if (knob != null)
            knob.OnStepChanged.RemoveListener(OnKnobStepChanged);
    }

    private void Start()
    {
        if (knob == null)
        {
            Debug.LogError("[Radio] No ClampedKnob assigned!", this);
            return;
        }

        // Resolve safe channel (0 means use center channel)
        // Convert from 1-indexed input to 0-indexed internal
        if (safeChannel <= 0) resolvedSafeChannelInternal = knob.StepAtAngleZero;
        else resolvedSafeChannelInternal = safeChannel - 1;
        
        resolvedSafeChannelInternal = Mathf.Clamp(resolvedSafeChannelInternal, 0, knob.Steps - 1);

        // Resolve starting channel (0 means use center channel)
        // Convert from 1-indexed input to 0-indexed internal
        int resolvedStartingInternal;
        
        if (startingChannel <= 0) resolvedStartingInternal = knob.StepAtAngleZero;
        else resolvedStartingInternal = startingChannel - 1;
        
        resolvedStartingInternal = Mathf.Clamp(resolvedStartingInternal, 0, knob.Steps - 1);

        // Set knob to starting channel (0-indexed)
        knob.SetStep(resolvedStartingInternal);
        currentChannelInternal = knob.CurrentStep;

        // Set initial safe channel state
        isOnSafeChannel = IsOnChannel(SafeChannel);

        UpdateDebugUI();
    }

    private void OnKnobStepChanged(int step)
    {
        ApplyChannel(step);
    }

    /// <summary>
    /// Sets the radio to a specific channel (1-indexed).
    /// </summary>
    public void SetChannel(int channel)
    {
        if (knob == null) return;

        // Convert 1-indexed to 0-indexed and set knob
        int step = Mathf.Clamp(channel - 1, 0, knob.Steps - 1);
        knob.SetStep(step);

        // Apply will be called via OnKnobStepChanged from the knob's event
    }

    private void ApplyChannel(int stepInternal)
    {
        if (stepInternal == currentChannelInternal) return;

        currentChannelInternal = stepInternal;
        isOnSafeChannel = IsOnChannel(SafeChannel);

        OnChannelChanged?.Invoke(CurrentChannel, isOnSafeChannel);
        UpdateDebugUI();
    }

    private void UpdateDebugUI()
    {
        if (channelText == null) return;
        channelText.text = $"CH: {CurrentFrequency}";
    }

    /// <summary>
    /// Checks if currently on a specific channel (1-indexed).
    /// </summary>
    public bool IsOnChannel(int channel)
    {
        return CurrentChannel == channel;
    }

    private void OnValidate()
    {
        if (totalChannels < 2) 
            totalChannels = 2;

        int maxChannel = overrideKnobSteps ? totalChannels : (knob != null ? knob.Steps : totalChannels);

        // Clamp 1-indexed values (1 to maxChannel, or 0 for auto)
        if (safeChannel > maxChannel) safeChannel = maxChannel;
        else if (safeChannel < 0) safeChannel = 0;

        if (startingChannel > maxChannel) startingChannel = maxChannel;
        else if (startingChannel < 0) startingChannel = 0;

        // Apply to knob in editor if override is enabled
        if (knob != null && overrideKnobSteps)
            knob.Steps = totalChannels;
    }

    #region Getters
    /// <summary>Current channel (1-indexed, 1 to TotalChannels)</summary>
    public int CurrentChannel => currentChannelInternal + 1;

    /// <summary>Current channel with prefix (e.g., 89, 90, 91...)</summary>
    public int CurrentFrequency => channelPrefix + CurrentChannel;

    /// <summary>Total number of channels</summary>
    public int TotalChannels => overrideKnobSteps ? totalChannels : (knob != null ? knob.Steps : 0);

    /// <summary>The safe channel (1-indexed)</summary>
    public int SafeChannel => resolvedSafeChannelInternal + 1;

    /// <summary>The channel that corresponds to angle 0 on the knob (1-indexed)</summary>
    public int CenterChannel => knob != null ? knob.StepAtAngleZero + 1 : 1;
    #endregion
}
