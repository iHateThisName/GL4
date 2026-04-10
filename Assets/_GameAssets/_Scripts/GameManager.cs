using Assets.Scripts.Singleton;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : PersistenSingleton<GameManager> {

    [Header("=== Runtime References ===")]
    [SerializeField] private SO_ScreenFadeRef screenFadeRef;

    [Header("=== Night Configuration ===")]
    [SerializeField] private SO_NightSettings nightSettings;

    // Event invoked whenever a scheduled night event becomes available.
    // Other systems can subscribe to react (e.g., spawning enemies, triggering sounds).
    public static event System.Action<NightEvent> OnEventAvailable = delegate { };
    
    private TimerHandle nightTimerHandle;
    private int night = 1;
    private int eventsFired = 0;

    private NightEventData[] eventsToFire;
    private ScheduledNightEvent[] schedule;
    private int scheduleCursor;

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
    }

    /// <summary>
    /// Unity callback invoked when the object becomes disabled.
    /// Unsubscribes the debug method to avoid memory leaks or duplicate logs.
    /// </summary>
    private void OnDisable()
    {
        DeathSystem.OnPlayerDied -= HandleNightEarlyEnd;
    }
    
    private void Start() {

        this.eventsToFire = this.nightSettings.GetEventsForNight(this.night);
        InstantiateTimer();
    }
    
    protected override void OnDestroy()
    {
        base.OnDestroy();
        TimerManager.Release(ref this.nightTimerHandle);
    }

    public void ContinueGame() {
        Debug.Log("Continuing Game...");
        // Add logic to continue the game from the game over scene
        
        SceneTransition.LoadScene(0, this.screenFadeRef);
        InstantiateTimer();
    }

    private void InstantiateTimer()
    {
        Debug.Log("Instantiating Timer...");
        TimerManager.Release(ref this.nightTimerHandle);

        // Resolve every event's per-event timing into an absolute schedule for this night.
        this.schedule = this.nightSettings.BuildScheduleForNight(this.night);
        this.scheduleCursor = 0;
        this.eventsFired = 0;

        float nightSeconds = this.nightSettings.GetNightTimeInSeconds();
        float firstInterval = this.schedule.Length > 0
            ? Mathf.Max(0.0001f, this.schedule[0].TimeSeconds)
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
        if (this.schedule == null || this.scheduleCursor >= this.schedule.Length)
        {
            ParkTimerUntilNightEnd();
            return;
        }

        float elapsed = TimerManager.Validate(this.nightTimerHandle) ? TimerManager.GetRef(this.nightTimerHandle).Elapsed : 0f;

        // Fire all events whose scheduled time has been reached this tick.
        while (this.scheduleCursor < this.schedule.Length
               && this.schedule[this.scheduleCursor].TimeSeconds <= elapsed + 0.0001f)
        {
            var scheduled = this.schedule[this.scheduleCursor];
            this.scheduleCursor++;
            this.eventsFired++;

            Debug.Log($"Night event fired at night {this.night}: {elapsed:F2}s (scheduled {scheduled.TimeSeconds:F2}s)");
            OnEventAvailable.Invoke(new NightEvent(scheduled.Data, this.eventsFired, this.night));
        }

        // Schedule the next tick to land exactly on the next pending event, or park
        // the timer past the end of the night if everything has fired.
        if (this.scheduleCursor >= this.schedule.Length)
        {
            ParkTimerUntilNightEnd();
            return;
        }

        if (TimerManager.Validate(this.nightTimerHandle))
        {
            ref var t = ref TimerManager.GetRef(this.nightTimerHandle);
            float delta = Mathf.Max(0.0001f, this.schedule[this.scheduleCursor].TimeSeconds - elapsed);
            t.Interval = delta;
            t.NextInterval = t.Elapsed + delta;
        }
    }

    private void ParkTimerUntilNightEnd()
    {
        if (this.nightSettings == null || !TimerManager.Validate(this.nightTimerHandle)) return;
        ref var t = ref TimerManager.GetRef(this.nightTimerHandle);
        t.Interval = this.nightSettings.GetNightTimeInSeconds() + 10f;
        t.NextInterval = t.Elapsed + t.Interval;
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
}
