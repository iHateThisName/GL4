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
        [SerializeField] private MonsterTransition[] transitions;

        // Auto-collected on Awake
        private MonsterState[] states;
        private MonsterSensor[] sensors;
        private Dictionary<System.Type, MonsterSensor> sensorCache;
        private Dictionary<string, float> timers;

        // Auto-collected component refs
        public Animator Animator { get; private set; }
        public AudioSource Audio { get; private set; }

        // Runtime
        public MonsterState CurrentState { get; private set; }
        public MonsterState PreviousState { get; private set; }
        public MonsterConfig Config => config;
        public int CurrentNight => GameManager.Instance != null ? GameManager.Instance.GetCurrentNight() : 1;
        public MonsterTransition[] Transitions => transitions;

        private void Awake()
        {
            CleanupBrokenConditions();
            timers = new Dictionary<string, float>();
            sensorCache = new Dictionary<System.Type, MonsterSensor>();

            Animator = GetComponentInChildren<Animator>();
            Audio = GetComponentInChildren<AudioSource>();

            // Auto-collect states from "States" child
            Transform statesRoot = transform.Find("States");
            if (statesRoot != null)
            {
                states = statesRoot.GetComponentsInChildren<MonsterState>(true);
            }
            else
            {
                states = GetComponentsInChildren<MonsterState>(true);
            }

            // Auto-collect sensors from this GO and children
            sensors = GetComponentsInChildren<MonsterSensor>(true);

            // Disable all states initially
            for (int i = 0; i < states.Length; i++)
            {
                states[i].enabled = false;
            }
        }

        private void OnEnable()
        {
            MonsterStateManager.Register(this);

            // Enter initial state
            if (initialState != null && CurrentState == null)
            {
                TransitionTo(initialState);
            }
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

        /// Get a sensor of type T with a specific ID (for multiple sensors of same type).
        public T GetSensor<T>(string id) where T : MonsterSensor
        {
            for (int i = 0; i < sensors.Length; i++)
            {
                if (sensors[i] is T typed && typed.SensorId == id)
                    return typed;
            }
            return null;
        }

        // --- Timer Accessors ---

        public float GetTimer(string key)
        {
            return timers.TryGetValue(key, out float val) ? val : 0f;
        }

        public void SetTimer(string key, float value)
        {
            timers[key] = value;
        }

        public void ResetTimer(string key)
        {
            timers[key] = 0f;
        }

        public void ResetAllTimers()
        {
            timers.Clear();
        }

        public void TickTimer(string key, float tickDelta, float rate = 1f)
            => SetTimer(key, GetTimer(key) + tickDelta * rate);

        // --- Config Accessors ---

        public T GetConfig<T>() where T : MonsterConfig => config as T;

        // --- State Machine ---

        /// Evaluate all transitions, return first match or null.
        public MonsterTransition EvaluateTransitions()
        {
            if (transitions == null) return null;

            for (int i = 0; i < transitions.Length; i++)
            {
                var t = transitions[i];
                if (t.toState == null) continue;

                // fromState == null means "any state"
                if (t.fromState != null && t.fromState != CurrentState) continue;

                if (t.Evaluate(this)) return t;
            }
            return null;
        }

        /// Tick all sensors.
        public void TickSensors()
        {
            for (int i = 0; i < sensors.Length; i++)
            {
                sensors[i].Tick(this);
            }
        }

        /// Perform a state transition.
        public void TransitionTo(MonsterState newState)
        {
            if (newState == null) return;

            if (CurrentState != null)
            {
                CurrentState.OnStateExit(this);
                CurrentState.enabled = false;
            }

            PreviousState = CurrentState;
            CurrentState = newState;
            CurrentState.enabled = true;
            CurrentState.OnStateEnter(this);
        }

        /// Swap the active config at runtime (for Mimic or night changes).
        public void SetConfig(MonsterConfig newConfig)
        {
            config = newConfig;
        }

        private void OnValidate()
        {
            // Cleanup of orphaned conditions (broken SerializeReference) happens
            // in Awake so it doesn't interfere with inspector authoring, where
            // newly added entries are legitimately null until a type is picked.
        }

        private void CleanupBrokenConditions()
        {
            if (transitions == null) return;
            for (int i = 0; i < transitions.Length; i++)
            {
                int removed = transitions[i].CleanupNullConditions();
                if (removed > 0)
                    Debug.LogWarning($"[MonsterController] Transition {i}: stripped {removed} broken condition(s).", this);
            }
        }
    }
}
