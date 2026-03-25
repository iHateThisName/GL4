using UnityEngine;

namespace MonsterSystem
{
    /// <summary>
    /// A sensor that listens for GameManager events and triggers state transitions.
    /// </summary>
    public class TimedSensor : MonsterSensor
    {
        [Header("Event Settings")]
        [Tooltip("The event type this sensor responds to")]
        [SerializeField] private GameManager.EventType respondToEventType = GameManager.EventType.DisruptRadio;

        protected override void Subscribe()
        {
            base.Subscribe();
            GameManager.OnEventAvailable += OnGameManagerEvent;
        }

        protected override void Unsubscribe()
        {
            base.Unsubscribe();
            GameManager.OnEventAvailable -= OnGameManagerEvent;
        }

        private void OnGameManagerEvent(GameManager.NightEvent evt)
        {
            Debug.Log("Hello radio should switch");
            var eventPayload = evt.GetPayload();
            if (eventPayload.GetEventType() != respondToEventType) return;

            // Trigger transition to the configured state
            TriggerStateTransition();
            Debug.Log("Radio should switch");
        }
    }
}
