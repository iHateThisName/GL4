using System.Collections.Generic;
using UnityEngine;

namespace MonsterSystem
{
    /// <summary>
    /// Represents a world-space spawn location defined by position and rotation.
    /// </summary>
    [System.Serializable]
    public struct SpawnPoint
    {
        public Vector3 position; // World-space position of the spawn point
        public Vector3 rotation; // Euler angles defining the spawn orientation
    }
    
    /// <summary>
    /// Defines per-night multipliers for patience, aggression, and speed,
    /// allowing the monster's difficulty to scale across successive nights.
    /// </summary>
    [System.Serializable]
    public struct NightOverride
    {
        public int nightNumber; // The night index this override applies to

        [Tooltip("1.0 = normal, 0.5 = half patience (harder)")]
        public float patienceMultiplier; // Scales how long the monster waits before acting; lower values increase difficulty

        [Tooltip("1.0 = normal, 2.0 = double aggression")]
        public float aggressionMultiplier; // Scales monster aggression; higher values increase difficulty

        [Tooltip("1.0 = normal speed")]
        public float speedMultiplier; // Scales monster movement speed; higher values make the monster faster
    }
    
    public class MonsterController : MonoBehaviour
    {
        [Header("Config")]
        public NightOverride[] nightOverrides; // Per-night difficulty multipliers for scaling monster behaviour
        [SerializeField] private SO_TransformCollection spawnPoints;
        [SerializeField] private SO_TransformRef playerRef;

        [Header("References")] 
        [SerializeField] protected Animator animator;
        [SerializeField] protected AudioSource audioSource;
        [SerializeField] private Transform sensorRoot;

        [Header("State Machine")]
        [SerializeField] private MonsterState initialState;

        // Auto-collected on Awake
        private Dictionary<System.Type, MonsterSensor> sensorCache;
        private MonsterState[] states;
        private MonsterSensor[] sensors;
        private MonsterState currentState;
        private MonsterState previousState;

        private void Awake()
        {
            this.sensorCache = new Dictionary<System.Type, MonsterSensor>();

            if (this.animator == null) this.animator = GetComponentInChildren<Animator>();
            if (this.audioSource == null) this.audioSource = GetComponentInChildren<AudioSource>();

            // Auto-collect states from "States" child
            Transform statesRoot = this.initialState == null ? transform.root.Find("States") : this.initialState.transform.parent;
            if (statesRoot != null)
                states = statesRoot.GetComponentsInChildren<MonsterState>(true);
            else
                states = GetComponentsInChildren<MonsterState>(true);

            Transform sensorRoot = this.sensorRoot != null ? this.sensorRoot : transform.root.Find("Sensors");
            this.sensors = sensorRoot.GetComponentsInChildren<MonsterSensor>(true);

            // Disable all state GameObjects immediately
            for (int i = this.states.Length - 1; i >= 0; i--)
            {
                this.states[i].gameObject.SetActive(false);
            }
        }

        private void Start()
        {
            // Initialize states and sensors in Start so all Awake methods have completed
            // (e.g., Radio registering itself in RuntimeReferences)
            for (int i = this.states.Length - 1; i >= 0; i--)
            {
                this.states[i].Initialize(this);
            }

            for (int i = this.sensors.Length - 1; i >= 0; i--)
            {
                this.sensors[i].Initialize(this);
            }

            // Enter initial state after initialization
            if (this.initialState != null && this.currentState == null)
            {
                TransitionTo(this.initialState);
            }
        }

        private void OnEnable()
        {
            MonsterStateManager.Register(this);
        }

        private void OnDisable()
        {
            MonsterStateManager.Deregister(this);
        }
        
        /// Tick all sensors with the elapsed time since last tick.
        public void TickSensors(float tickDelta)
        {
            for (int i = 0; i < this.sensors.Length; i++)
            {
                this.sensors[i].OnTick(tickDelta);
            }
        }

        /// Perform a state transition.
        public void TransitionTo(MonsterState newState)
        {
            if (newState == null) return;
            
            this.previousState = this.currentState;
            if (this.previousState != null)
            {
                this.previousState.OnStateExit();
                this.previousState.gameObject.SetActive(false);
            }

            this.currentState = newState;
            this.currentState.gameObject.SetActive(true);
            this.currentState.OnStateEnter();

            // Notify sensors that state changed so they can trigger again
            NotifySensorsStateChanged();
        }

        /// <summary>
        /// Perform a state transition with typed context data.
        /// If the target state implements IStateWithContext&lt;T&gt;, it will receive the context
        /// before OnStateEnter is called.
        /// </summary>
        public void TransitionTo<T>(MonsterState newState, T context)
        {
            if (newState == null) return;
            
            this.previousState = this.currentState;
            if (this.previousState != null)
            {
                this.previousState.OnStateExit();
                this.previousState.gameObject.SetActive(false);
            }

            this.currentState = newState;
            this.currentState.gameObject.SetActive(true);

            // Pass context to the state if it implements the interface
            if (newState is IStateWithContext<T> contextState)
                contextState.ReceiveContext(context);

            this.currentState.OnStateEnter();

            // Notify sensors that state changed so they can trigger again
            NotifySensorsStateChanged();
        }
        
        private void NotifySensorsStateChanged()
        {
            for (int i = 0; i < this.sensors.Length; i++)
            {
                this.sensors[i].OnStateChanged();
            }
        }

        // --- Getters ---
        public T GetSensor<T>() where T : MonsterSensor
        {
            System.Type type = typeof(T);
            if (this.sensorCache.TryGetValue(type, out MonsterSensor cached))
                return (T)cached;

            for (int i = 0; i < this.sensors.Length; i++)
            {
                if (this.sensors[i] is T typed)
                {
                    this.sensorCache[type] = this.sensors[i];
                    return typed;
                }
            }
            return null;
        }
        
        public T GetMonsterState<T>() where T : MonsterState
        {
            for (int i = 0; i < states.Length; i++)
            {
                if (states[i] is T typed)
                    return typed;
            }
            return null;
        }
        
        public MonsterState CurrentState => this.currentState;

        public MonsterState PreviousState => this.previousState;
        
        public Animator Animator => this.animator;
        
        public AudioSource Audio => this.audioSource;
        
        public SO_TransformCollection SpawnPoints => this.spawnPoints;
        
        public int CurrentNight => GameManager.Instance != null ? GameManager.Instance.GetCurrentNight() : 1;
        
        /// <summary>
        /// Returns true if the current state is blocking external transitions (e.g., from sensors).
        /// </summary>
        public bool IsBlockingTransitions => this.currentState != null && this.currentState.BlocksTransitions;
        
        public Transform PlayerTarget => this.playerRef != null ? this.playerRef.Value : null;
        
        /// <summary>
        /// Searches the night overrides array for a matching night number and returns it.
        /// If no override is defined for the given night, returns a neutral override with all multipliers set to 1.
        /// </summary>
        /// <param name="night">The night number to look up.</param>
        /// <returns>The <see cref="NightOverride"/> for the specified night, or a default neutral override.</returns>
        public NightOverride GetOverrideForNight(int night)
        {
            // Search configured overrides for a matching night number
            if (this.nightOverrides != null)
            {
                for (int i = 0; i < this.nightOverrides.Length; i++)
                {
                    if (this.nightOverrides[i].nightNumber == night)
                        return this.nightOverrides[i];
                }
            }

            // No override found; return neutral multipliers (no scaling)
            return new NightOverride
            {
                nightNumber = night,
                patienceMultiplier = 1f,
                aggressionMultiplier = 1f,
                speedMultiplier = 1f
            };
        }
    }
}
