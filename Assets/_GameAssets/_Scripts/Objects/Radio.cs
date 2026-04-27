using System;
using System.Collections.Generic;
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

    [SerializeField] private int displayNumber = 0;

    [SerializeField] private TutorialManager tutorialManager;

    // Internal channel is 0-indexed
    private int currentChannelInternal = -1;
    // Resolved safe channel is 0-indexed
    private int resolvedSafeChannelInternal;
    private bool isOnSafeChannel;
    // Guards against callbacks arriving before Start() finishes
    private bool initialized;

    private struct QueuedBroadcast
    {
        public AudioClip clip;
        public float resumeTime;

        public QueuedBroadcast(AudioClip clip, float resumeTime)
        {
            this.clip = clip;
            this.resumeTime = resumeTime;
        }
    }

    private readonly LinkedList<QueuedBroadcast> broadcastQueue = new();
    private bool isPlayingBroadcast;

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
        if (editorTestMode && initialized && knob != null)
            knob.value = knob.value; // re-triggers SetValue → fires onValueChange

        if (isPlayingBroadcast && !audioSource.isPlaying)
        {
            isPlayingBroadcast = false;
            PlayNextBroadcast();
        }
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

        switch (startingChannel)
        {
            case 1:
                displayNumber = 27;
                break;
            case 2:
                displayNumber = 28;
                break;
            case 3:
                displayNumber = 29;
                break;
            case 4:
                displayNumber = 30;
                break;
            case 5:
                displayNumber = 31;
                break;
            case 6:
                displayNumber = 32;
                break;
            case 7:
                displayNumber = 33;
                break;
            case 8:
                displayNumber = 34;
                break;
            case 9:
                displayNumber = 35;
                break;
            default:
                displayNumber = 30;
                break;
        }


        // Set knob to starting channel — suppress event callbacks during init
        knob.value = StepToValue(resolvedStartingInternal);
        currentChannelInternal = ValueToStep(knob.value);
        isOnSafeChannel = IsOnChannel(SafeChannel);

        initialized = true;
        UpdateDebugUI();
    }

    private void OnNightEvent(NightEvent evt)
    {
        if (evt.GetEventType() != NightEvent.NightEventType.RadioBroadcast) return;
        Debug.Log("Radio received broadcast event and is safe: " + isOnSafeChannel);

        if (!isOnSafeChannel) return;

        int tipIndex = UnityEngine.Random.Range(0, tips.Length);
        AudioClip incoming = tips[tipIndex];
        Debug.Log("Queuing/playing tip: " + tipIndex + " for " + incoming.name);

        if (isPlayingBroadcast)
        {
            if (evt.GetIsOverrideBroadcast())
            {
                // Pause current clip and push it to the front of the queue so it resumes after the override.
                float pausedTime = audioSource.time;
                AudioClip pausedClip = audioSource.clip;
                audioSource.Stop();
                broadcastQueue.AddFirst(new QueuedBroadcast(pausedClip, pausedTime));
                isPlayingBroadcast = false;
                PlayBroadcast(new QueuedBroadcast(incoming, 0));
            }
            else
            {
                broadcastQueue.AddLast(new QueuedBroadcast(incoming, 0));
                Debug.Log("Broadcast queued. Queue size: " + broadcastQueue.Count);
            }
        }
        else
        {
            PlayBroadcast(new QueuedBroadcast(incoming, 0));
        }
    }

    private void PlayBroadcast(QueuedBroadcast broadcast)
    {
        isPlayingBroadcast = true;
        audioSource.clip = broadcast.clip;
        audioSource.time = broadcast.resumeTime;
        audioSource.loop = false;
        audioSource.Play();
    }

    private void PlayNextBroadcast()
    {
        if (broadcastQueue.Count == 0) return;
        var next = broadcastQueue.First.Value;
        broadcastQueue.RemoveFirst();
        PlayBroadcast(next);
    }

    private void ClearBroadcastQueue()
    {
        broadcastQueue.Clear();
        isPlayingBroadcast = false;
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
            ClearBroadcastQueue();
            if (!audioSource.isPlaying || audioSource.clip != staticSound)
            {
                audioSource.clip = staticSound;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
        else
        {
            Debug.Log("Radio stopped playing static sound");
            this.audioSource.Stop();
            this.audioSource.loop = false;
            this.audioSource.clip = null;
            if(tutorialManager != null && !tutorialManager.hasFixedRadio)
            {
                tutorialManager.FixRadio();
            }
        }
        OnChannelChanged?.Invoke(CurrentChannel, isOnSafeChannel);
        switch(CurrentChannel)
        {
            case 1:
                displayNumber = 27;
                break;
            case 2:
                displayNumber = 28;
                break;
            case 3:
                displayNumber = 29;
                break;
            case 4:
                displayNumber = 30;
                break;
            case 5:
                displayNumber = 31;
                break;
            case 6:
                displayNumber = 32;
                break;
            case 7:
                displayNumber = 33;
                break;
            case 8:
                displayNumber = 34;
                break;
            case 9:
                displayNumber = 35;
                break;
            default:
                break;
        }
        UpdateDebugUI();
    }

    private void UpdateDebugUI()
    {
        if (channelText == null) return;
        channelText.text = $"CH: {displayNumber}";
    }

    public bool IsOnChannel(int channel)
    {
        return CurrentChannel == channel;
    }

    [ContextMenu("Set Random Channel")]
    private void ContextSetRandomChannel()
    {
        int channel;
        do { channel = UnityEngine.Random.Range(1, TotalChannels + 1); }
        while (channel == SafeChannel && TotalChannels > 1);
        SetChannel(channel);
    }

    [ContextMenu("Set Safe Channel")]
    private void ContextSetSafeChannel()
    {
        SetChannel(SafeChannel);
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
