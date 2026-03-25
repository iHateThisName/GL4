using Assets.Scripts.Singleton;
using Gaskellgames;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : PersistenSingleton<GameManager> {

    [Header("Prefab Refrences")]
    [SerializeField, AssetsOnly] private GameObject FireAdaptationVolumePrefab;
    [HideInInspector] public FireAdaptationController FireAdaptationController { get; private set; }

    [Header("=== Runtime References ===")]
    [SerializeField] private SO_ScreenFadeRef screenFadeRef;
    [SerializeField] private SO_TransformRef playerRef;
    [SerializeField] private Transform player;

    [Header("=== Night Configuration ===")]
    [SerializeField] private SO_NightSettings nightSettings;

    // Event invoked whenever a scheduled night event becomes available.
    // Other systems can subscribe to react (e.g., spawning enemies, triggering sounds).
    public static event System.Action<NightEvent> OnEventAvailable = delegate { };
    
    private TimerHandle nightTimerHandle;
    private int night = 1;
    private int eventsFired = 0;

    private NightEventData[] eventsToFire;

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
        OnEventAvailable += DebugEventTimeWorking;
        DeathSystem.OnPlayerDied += HandleNightEarlyEnd;
    }

    /// <summary>
    /// Unity callback invoked when the object becomes disabled.
    /// Unsubscribes the debug method to avoid memory leaks or duplicate logs.
    /// </summary>
    private void OnDisable()
    {
        OnEventAvailable -= DebugEventTimeWorking;
        DeathSystem.OnPlayerDied -= HandleNightEarlyEnd;
    }
    
    private void Start() {
        if (this.playerRef != null && this.player != null)
            this.playerRef.Value = this.player;

        // use the existing instance if it exists in the scene
        this.FireAdaptationController = FindFirstObjectByType<FireAdaptationController>();
        if (this.FireAdaptationController == null) {
            // otherwise instantiate a new one from the prefab
            this.FireAdaptationController = Instantiate(this.FireAdaptationVolumePrefab).GetComponent<FireAdaptationController>();
        }

        InstantiateTimer();
        this.eventsToFire = this.nightSettings.GetEventsForNight(this.night);
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
        this.nightTimerHandle = TimerManager.Create(this.nightSettings.GetNewNightEventTime(), this.nightSettings.GetNightTimeInSeconds());
        TimerManager.SetCallbacks(this.nightTimerHandle, HandleNightTick, HandleNightEnd);
        this.eventsFired = 0;
    }
    
    /// <summary>
    /// Updates the night timer and checks whether an event should fire.
    /// Stops updating once the night duration has been reached.
    /// </summary>
    private void HandleNightTick()
    {
        if (this.eventsFired + 1 > this.eventsToFire.Length)
        {
            if (this.nightSettings != null && TimerManager.Validate(this.nightTimerHandle))
            {
                ref var t = ref TimerManager.GetRef(this.nightTimerHandle);
                t.Interval = this.nightSettings.GetNightTimeInSeconds() + 10;
                t.NextInterval = t.Elapsed + t.Interval;
            }
            return;
        }

        float elapsed = TimerManager.Validate(this.nightTimerHandle) ? TimerManager.GetRef(this.nightTimerHandle).Elapsed : 0f;
        Debug.Log($"Night event fired at: {this.night}: {elapsed}s");
        this.eventsFired++;

        OnEventAvailable.Invoke(new NightEvent(this.eventsToFire[this.eventsFired - 1], this.eventsFired, this.night));
        if (this.nightSettings != null && TimerManager.Validate(this.nightTimerHandle))
        {
            ref var t = ref TimerManager.GetRef(this.nightTimerHandle);
            t.Interval = this.nightSettings.GetNewNightEventTime();
            t.NextInterval = t.Elapsed + t.Interval;
        }
    }

    private void HandleNightEnd()
    {
        Debug.Log("Night Survived");
        this.night++;

        TimerManager.Release(ref this.nightTimerHandle);

        DeathSystem.KillPlayer(DeathSystem.DeathEvent.DeathReason.Survived, false);

        if (this.night == this.nightSettings.GetFinalNight())
            DeathSystem.WinGame();
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

    /// <summary>
    /// Debug helper method that logs when an event fires and when the next one is scheduled.
    /// Useful for verifying timing behavior during development.
    /// </summary>
    private void DebugEventTimeWorking(NightEvent eventData)
    {
        //Debug.Log($"Event fired at {eventData}"); Yes is working relax.
    }



    [System.Serializable]
    public enum EventType
    {
        SpawnMonster,
        SpawnFood,
        DisruptRadio,
    }
    
    [System.Serializable]
    public struct NightEventData
    {
        [SerializeField] private GameManager.EventType eventType;
        [SerializeField] private GameObject monster;
        [SerializeField] private int monsterCount;
        
        public GameManager.EventType GetEventType() => this.eventType;
        public GameObject GetMonsterPrefab() => this.monster;
        public int GetMonsterCount() => this.monsterCount;
    }
    
    [System.Serializable]
    public struct NightEvent
    {
        [SerializeField] private int eventIdx;
        [SerializeField] private int night;
        [SerializeField] private NightEventData eventData;
        
        public NightEvent(NightEventData eventData, int idx, int night) 
        {
            this.eventIdx = idx;
            this.night = night;
            this.eventData = eventData;
        }
        
        public int Index => this.eventIdx;

        public int Night => this.night;

        public NightEventData GetPayload() => this.eventData;
    }
}
