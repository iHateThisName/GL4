using Assets.Scripts.Singleton;
using Gaskellgames;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : PersistenSingleton<GameManager> {

    [Header("Prefab Refrences")]
    [SerializeField, AssetsOnly] private GameObject FireAdaptationVolumePrefab;
    [HideInInspector] public FireAdaptationController FireAdaptationController { get; private set; }

    [Header("=== Night Configuration ===")] 
    [SerializeField] private SO_NightSettings nightSettings;

    // Event invoked whenever a scheduled night event becomes available.
    // Other systems can subscribe to react (e.g., spawning enemies, triggering sounds).
    public static event System.Action<NightEvent> OnEventAvailable = delegate { };
    
    private Timer nightTimer;
    private int night = 1;
    private int eventsFired = 0;

    private NightEventData[] eventsToFire;
    
    public float NightTime => this.nightTimer != null && this.nightTimer.IsRunning ? this.nightTimer.Elapsed : this.nightSettings.GetNightTimeInSeconds();

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
        // use the existing instance if it exists in the scene
        this.FireAdaptationController = FindFirstObjectByType<FireAdaptationController>();
        if (this.FireAdaptationController == null) {
            // otherwise instantiate a new one from the prefab
            this.FireAdaptationController = Instantiate(this.FireAdaptationVolumePrefab).GetComponent<FireAdaptationController>();
        }

        InstantiateTimer();
        this.eventsToFire = this.nightSettings.GetEventsForNight(this.night);
    }
    
    /// <summary>
    /// Clean up the timer when this component is destroyed.
    /// </summary>
    protected override void OnDestroy()
    {
        base.OnDestroy();
        
        if (this.nightTimer != null)
        {
            this.nightTimer.Dispose();
            this.nightTimer = null;
        }
    }

    public void ContinueGame() {
        Debug.Log("Continuing Game...");
        // Add logic to continue the game from the game over scene
        
        SceneManager.LoadScene("CabinLayoutFinal");
        InstantiateTimer();
    }

    private void InstantiateTimer()
    {
        Debug.Log("Instantiating Timer...");
        this.nightTimer = new Timer(this.nightSettings.GetNewNightEventTime(), this.nightSettings.GetNightTimeInSeconds());
        this.nightTimer.OnTimerTick += HandleNightTick;
        this.nightTimer.OnTimerFinished += HandleNightEnd;
        this.nightTimer.Start();
        this.eventsFired = 0;
    }
    
    /// <summary>
    /// Updates the night timer and checks whether an event should fire.
    /// Stops updating once the night duration has been reached.
    /// </summary>
    private void HandleNightTick()
    {
        // No need to fire any new events if we have fired of configured events
        if (this.eventsFired + 1 > this.eventsToFire.Length)
        {
            if (this.nightTimer != null && this.nightSettings != null)
                this.nightTimer.SetInterval(this.nightSettings.GetNightTimeInSeconds() + 10);
            return;
        }
        
        Debug.Log($"Night event fired at: {this.night}: {this.nightTimer.Elapsed}s");
        this.eventsFired++;
        
        OnEventAvailable.Invoke(new NightEvent(this.eventsToFire[this.eventsFired - 1], this.eventsFired, this.night)); // Notify subscribers
        if (this.nightTimer != null && this.nightSettings != null)
            this.nightTimer.SetInterval(this.nightSettings.GetNewNightEventTime()); // Schedule the next event
    }

    private void HandleNightEnd()
    {
        Debug.Log("Night Survived");
        this.night++;
        
        if (this.nightTimer != null)
            this.nightTimer.Dispose();
        
        DeathSystem.KillPlayer(DeathSystem.DeathEvent.DeathReason.Survived, false);
        
        if (this.night == this.nightSettings.GetFinalNight())
            DeathSystem.WinGame();
    }

    private void HandleNightEarlyEnd()
    {
        if (this.nightTimer != null)
            this.nightTimer.Dispose();

        if (DeathSystem.deathEvent.Reason != DeathSystem.DeathEvent.DeathReason.Survived)
            this.night = 1;
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
