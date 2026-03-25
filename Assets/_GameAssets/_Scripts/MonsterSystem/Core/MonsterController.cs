using System.Collections.Generic;
using UnityEngine;

namespace MonsterSystem
{
    public class MonsterController : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private MonsterConfig config;

        [Header("State Machine")]
        [SerializeField] private MonsterState initialState;

        // Auto-collected on Awake
        private MonsterState[] states;
        private MonsterSensor[] sensors;
        private Dictionary<System.Type, MonsterSensor> sensorCache;

        // Auto-collected component refs
        [field:SerializeField] public Animator Animator { get; private set; }
        [field: SerializeField] public AudioSource Audio { get; private set; }
        [SerializeField] private Transform sensorRoot;

        // Runtime
        public MonsterState CurrentState { get; private set; }
        public MonsterState PreviousState { get; private set; }
        public MonsterConfig Config => config;
        public int CurrentNight => GameManager.Instance != null ? GameManager.Instance.GetCurrentNight() : 1;

        private void Awake()
        {
            this.sensorCache = new Dictionary<System.Type, MonsterSensor>();

            if (this.Animator == null) this.Animator = GetComponentInChildren<Animator>();
            if (this.Audio == null) this.Audio = GetComponentInChildren<AudioSource>();

            // Auto-collect states from "States" child
            Transform statesRoot = this.initialState == null ? transform.root.Find("States") : this.initialState.transform.parent;
            if (statesRoot != null)
                states = statesRoot.GetComponentsInChildren<MonsterState>(true);
            else
                states = GetComponentsInChildren<MonsterState>(true);

            Transform sensorRoot = this.sensorRoot != null ? this.sensorRoot : transform.root.Find("Sensors");
            this.sensors = sensorRoot.GetComponentsInChildren<MonsterSensor>(true);

            // Disable all state GameObjects immediately
            for (int i = states.Length - 1; i >= 0; i--)
            {
                states[i].gameObject.SetActive(false);
            }
        }

        private void Start()
        {
            // Initialize states and sensors in Start so all Awake methods have completed
            // (e.g., Radio registering itself in RuntimeReferences)
            for (int i = states.Length - 1; i >= 0; i--)
            {
                states[i].Initialize(this);
            }

            for (int i = sensors.Length - 1; i >= 0; i--)
            {
                sensors[i].Initialize(this);
            }

            // Enter initial state after initialization
            if (initialState != null && CurrentState == null)
            {
                TransitionTo(initialState);
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

        // --- Sensor Accessors ---

        /// Get the first sensor of type T.
        public T GetSensor<T>() where T : MonsterSensor
        {
            System.Type type = typeof(T);
            if (sensorCache.TryGetValue(type, out MonsterSensor cached))
                return (T)cached;

            for (int i = 0; i < sensors.Length; i++)
            {
                if (sensors[i] is T typed)
                {
                    sensorCache[type] = sensors[i];
                    return typed;
                }
            }
            return null;
        }
        
        // --- Config Accessors ---

        public T GetConfig<T>() where T : MonsterConfig => config as T;


        public T GetMonsterState<T>() where T : MonsterState
        {
            for (int i = 0; i < states.Length; i++)
            {
                if (states[i] is T typed)
                    return typed;
            }
            return null;
        }

        // --- State Machine ---

        /// Tick all sensors with the elapsed time since last tick.
        public void TickSensors(float tickDelta)
        {
            for (int i = 0; i < sensors.Length; i++)
            {
                sensors[i].OnTick(tickDelta);
            }
        }

        /// Perform a state transition.
        public void TransitionTo(MonsterState newState)
        {
            if (newState == null) return;

            if (CurrentState != null)
            {
                CurrentState.OnStateExit();
                CurrentState.gameObject.SetActive(false);
            }

            PreviousState = CurrentState;
            CurrentState = newState;
            CurrentState.gameObject.SetActive(true);
            CurrentState.OnStateEnter();

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

            if (CurrentState != null)
            {
                CurrentState.OnStateExit();
                CurrentState.gameObject.SetActive(false);
            }

            PreviousState = CurrentState;
            CurrentState = newState;
            CurrentState.gameObject.SetActive(true);

            // Pass context to the state if it implements the interface
            if (newState is IStateWithContext<T> contextState)
                contextState.ReceiveContext(context);

            CurrentState.OnStateEnter();

            // Notify sensors that state changed so they can trigger again
            NotifySensorsStateChanged();
        }

        /// <summary>
        /// Returns true if the current state is blocking external transitions (e.g., from sensors).
        /// </summary>
        public bool IsBlockingTransitions => CurrentState != null && CurrentState.BlocksTransitions;

        private void NotifySensorsStateChanged()
        {
            for (int i = 0; i < sensors.Length; i++)
            {
                sensors[i].OnStateChanged();
            }
        }

        /// Swap the active config at runtime (for Mimic or night changes).
        public void SetConfig(MonsterConfig newConfig)
        {
            config = newConfig;
        }
    }
}
