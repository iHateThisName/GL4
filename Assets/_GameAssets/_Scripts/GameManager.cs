using Assets.Scripts.Singleton;
using Gaskellgames;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : PersistenSingleton<GameManager> {

    [Header("Prefab Refrences")]
    [SerializeField, AssetsOnly] private GameObject FireAdaptationVolumePrefab;
    [HideInInspector] public FireAdaptationController FireAdaptationController { get; private set; }

    [Header("=== Night Configuration ===")] 
    [SerializeField] private NightSettings nightSettings;

    // Event invoked whenever a scheduled night event becomes available.
    // Other systems can subscribe to react (e.g., spawning enemies, triggering sounds).
    public static event System.Action<NightEventData> OnEventAvailable = delegate { };
    
    private Timer nightTimer;
    private int night = 1;
    private int eventsFired = 0;
    
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
        this.nightTimer = new Timer(0, this.nightSettings.GetNightTimeInSeconds());
        this.nightTimer.OnTimerTick += HandleNightTick;
        this.nightTimer.OnTimerFinished += HandleNightEnd;
        this.nightTimer.Start();
    }
    
    /// <summary>
    /// Updates the night timer and checks whether an event should fire.
    /// Stops updating once the night duration has been reached.
    /// </summary>
    private void HandleNightTick()
    {
        Debug.Log($"Night event fired at: {this.night}: {this.nightTimer.Elapsed}s");
        this.eventsFired++;
        OnEventAvailable.Invoke(new NightEventData(this.eventsFired, this.night)); // Notify subscribers
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
    }

    /// <summary>
    /// Debug helper method that logs when an event fires and when the next one is scheduled.
    /// Useful for verifying timing behavior during development.
    /// </summary>
    private void DebugEventTimeWorking(NightEventData eventData)
    {
        //Debug.Log($"Event fired at {eventData}"); Yes is working relax.
    }
    
    [System.Serializable]
    public struct NightEventData
    {
        public int eventIdx;
        public int night;
        
        public NightEventData(int idx, int night) 
        {
            this.eventIdx = idx;
            this.night = night;
        }
    }
}
