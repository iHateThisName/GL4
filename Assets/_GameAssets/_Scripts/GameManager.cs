using Assets.Scripts.Singleton;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : PersistenSingleton<GameManager> {

    [Header("=== Runtime References ===")]
    [SerializeField] private SO_ScreenFadeRef screenFadeRef;

    [Header("=== Night Configuration ===")]
    [SerializeField] private SO_NightSettings nightSettings;
    [SerializeField] private bool debugSpawn;

    // Event invoked whenever a scheduled night event becomes available.
    // Other systems can subscribe to react (e.g., spawning enemies, triggering sounds).
    public static event System.Action<NightEvent> OnEventAvailable = delegate { };

    private TimerHandle nightTimerHandle;
    private int night = 1;
    private int eventsFired = 0;

    private ScheduledNightEvent[] eventsSchedule;
    private int scheduleCursor;

    [System.Serializable]
    private struct NightEventDebugView
    {
        public NightEventType EventType;
        public float TimeSeconds;
        public int OriginalIndex;
    }
    [SerializeField] private NightEventDebugView[] scheduleDebugView;

    public Dictionary<WindowController, VRLever.EnumLeverState> WindowsDictonary { get; private set; } = new Dictionary<WindowController, VRLever.EnumLeverState>();

    public float NightTime => TimerManager.Validate(this.nightTimerHandle) && TimerManager.GetRef(this.nightTimerHandle).IsRunning == 1
        ? TimerManager.GetRef(this.nightTimerHandle).Elapsed
        : this.nightSettings.GetNightTimeInSeconds();

    /// <summary>
    /// Unity callback invoked when the object becomes enabled.
    /// Subscribes a debug method to the event for testing purposes.
    /// </summary>
    private void OnEnable()
    {
        DeathSystem.OnPlayerDied += HandleNightEarlyEnd;
        SceneTransition.OnTransitionComplete += OnSceneTransitionComplete;
    }

    /// <summary>
    /// Unity callback invoked when the object becomes disabled.
    /// Unsubscribes the debug method to avoid memory leaks or duplicate logs.
    /// </summary>
    private void OnDisable()
    {
        DeathSystem.OnPlayerDied -= HandleNightEarlyEnd;
        SceneTransition.OnTransitionComplete -= OnSceneTransitionComplete;
    }

    private void Start() {
        this.night = this.nightSettings.DebugStartNight;
        InitializeNight();
    }

    private void OnSceneTransitionComplete(int sceneIndex)
    {
        Debug.Log($"[GameManager] loaded into: {sceneIndex}");
        if (sceneIndex == 1)
            InitializeNight();
    }

    private void InitializeNight()
    {
        this.eventsFired = 0;
        this.eventsSchedule = this.debugSpawn
            ? BuildDebugSpawnSchedule()
            : this.nightSettings.BuildScheduleForNight(this.night);
        RefreshScheduleDebugView();
        
        this.WindowsDictonary.Clear();
        InstantiateTimer();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        TimerManager.Release(ref this.nightTimerHandle);
    }

    public void ContinueGame() {
        Debug.Log("Continuing Game...");
        SceneTransition.LoadScene(1, this.screenFadeRef);
    }

    private void InstantiateTimer()
    {
        Debug.Log("Instantiating Timer...");
        TimerManager.Release(ref this.nightTimerHandle);
        
        this.scheduleCursor = 0;
        this.eventsFired = 0;

        float nightSeconds = this.nightSettings.GetNightTimeInSeconds();
        float firstInterval = this.eventsSchedule.Length > 0
            ? Mathf.Max(0.0001f, this.eventsSchedule[0].TimeSeconds)
            : nightSeconds + 10f;

        this.nightTimerHandle = TimerManager.Create(firstInterval, nightSeconds);
        TimerManager.SetCallbacks(this.nightTimerHandle, HandleNightTick, HandleNightEnd);
    }

    /// <summary>
    /// Fires every scheduled event whose time has been reached on this tick (so events
    /// sharing the same resolved time fire together), then schedules the next tick to
    /// land on the next pending event.
    /// </summary>
    private void HandleNightTick()
    {
        if (this.eventsSchedule == null || this.scheduleCursor >= this.eventsSchedule.Length)
        {
            ParkTimerUntilNightEnd();
            return;
        }

        float elapsed = TimerManager.Validate(this.nightTimerHandle) ? TimerManager.GetRef(this.nightTimerHandle).Elapsed : 0f;

        // Fire all events whose scheduled time has been reached this tick.
        while (this.scheduleCursor < this.eventsSchedule.Length
               && this.eventsSchedule[this.scheduleCursor].TimeSeconds <= elapsed + 0.0001f)
        {
            var scheduled = this.eventsSchedule[this.scheduleCursor];
            this.scheduleCursor++;
            this.eventsFired++;

            Debug.Log($"Night event fired at night {this.night}: {elapsed:F2}s (scheduled {scheduled.TimeSeconds:F2}s)");
            OnEventAvailable.Invoke(new NightEvent(scheduled.Data, this.eventsFired, this.night));
        }

        // Schedule the next tick to land exactly on the next pending event, or park
        // the timer past the end of the night if everything has fired.
        if (this.scheduleCursor >= this.eventsSchedule.Length)
        {
            ParkTimerUntilNightEnd();
            return;
        }

        if (TimerManager.Validate(this.nightTimerHandle))
        {
            ref var timer = ref TimerManager.GetRef(this.nightTimerHandle);
            float delta = Mathf.Max(0.0001f, this.eventsSchedule[this.scheduleCursor].TimeSeconds - elapsed);
            timer.Interval = delta;
            timer.NextInterval = timer.Elapsed + delta;
        }
    }

    private void RefreshScheduleDebugView()
    {
        this.scheduleDebugView = new NightEventDebugView[this.eventsSchedule.Length];
        for (int i = 0; i < this.eventsSchedule.Length; i++)
            this.scheduleDebugView[i] = new NightEventDebugView
            {
                EventType = this.eventsSchedule[i].Data.GetEventType(),
                TimeSeconds = this.eventsSchedule[i].TimeSeconds,
                OriginalIndex = this.eventsSchedule[i].OriginalIndex
            };
    }

    /// <summary>
    /// Collects every SpawnMonster event from all nights and schedules them all at 0.1 s,
    /// ignoring their configured timing and which night they belong to.
    /// </summary>
    private ScheduledNightEvent[] BuildDebugSpawnSchedule()
    {
        var result = new List<ScheduledNightEvent>();
        int idx = 0;

        this.nightSettings.ForEachEventAcrossAllNights(evt =>
        {
            if (evt.GetEventType() == NightEventType.SpawnMonster)
                result.Add(new ScheduledNightEvent(evt, 0.1f, idx++));
        });
        
        return result.ToArray();
    }

    private void ParkTimerUntilNightEnd()
    {
        if (this.nightSettings == null || !TimerManager.Validate(this.nightTimerHandle)) return;
        ref var timer = ref TimerManager.GetRef(this.nightTimerHandle);
        timer.Interval = this.nightSettings.GetNightTimeInSeconds() + 10f;
        timer.NextInterval = timer.Elapsed + timer.Interval;
    }

    private void HandleNightEnd()
    {
        Debug.Log("Night Survived");
        this.night++;

        TimerManager.Release(ref this.nightTimerHandle);

        DeathSystem.KillPlayer(DeathSystem.DeathEvent.DeathReason.Survived, "", false);

        // final night finish game
        if (this.night == this.nightSettings.GetFinalNight()) {}
    }

    private void HandleNightEarlyEnd()
    {
        TimerManager.Release(ref this.nightTimerHandle);

        if (DeathSystem.deathEvent.Reason != DeathSystem.DeathEvent.DeathReason.Survived)
            this.night = 1;
    }

    public int GetCurrentNight() => this.night;

    public void UpdateWindowState(WindowController windowController, VRLever.EnumLeverState newSate) {
        // update the dictionary with the new state and remove the old refrence
        this.WindowsDictonary.Remove(windowController);
        this.WindowsDictonary.Add(windowController, newSate);

        PlayerTemperatureSimulator.Instance.UpdateOpenWindowCount();
    }

    public List<WindowController> GetClosedWindows() {
        List<WindowController> closedWindows = new List<WindowController>();
        foreach (KeyValuePair<WindowController, VRLever.EnumLeverState> kvp in this.WindowsDictonary) {
            if (kvp.Value == VRLever.EnumLeverState.Closed) {
                closedWindows.Add(kvp.Key);
            }
        }
        return closedWindows;
    }

    [ContextMenu("Debug Log Window Statuses")]
    public void DebugLogWindowStatuses() {
        int openCount = 0;
        int closedCount = 0;
        int smartUpdateEnabledCount = 0;
        string logMessage = "Current Window States:\n";
        foreach (KeyValuePair<WindowController, VRLever.EnumLeverState> kvp in this.WindowsDictonary) {
            if (kvp.Value == VRLever.EnumLeverState.Open) openCount++;
            else if (kvp.Value == VRLever.EnumLeverState.Closed) closedCount++;

            if (kvp.Key.IsVRLeverSmartUpdateEnabled()) smartUpdateEnabledCount++;
        }
        logMessage += $"Total Closed Windows: {closedCount} out of {this.WindowsDictonary.Count}\n";
        logMessage += $"Total Open Windows: {openCount} out of {this.WindowsDictonary.Count}\n";
        logMessage += $"Windows with Smart Update Enabled: {smartUpdateEnabledCount} out of {this.WindowsDictonary.Count}\n";
        Debug.Log(logMessage);
    }
}
