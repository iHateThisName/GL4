using UnityEngine;

namespace MonsterSystem
{
    /// <summary>
    /// A sensor that listens for GameManager events and triggers state transitions.
    /// </summary>
    public class NightEventSensor : MonsterSensor
    {
        [Header("Event Settings")]
        [Tooltip("The event type this sensor responds to")]
        [SerializeField] private NightEventType respondToEventType = NightEventType.DisruptRadio;

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

        private void OnGameManagerEvent(NightEvent evt)
        {
            var eventPayload = evt.GetPayload();
            if (eventPayload.GetEventType() != respondToEventType) return;

            // Trigger transition to the configured state
            TriggerStateTransition();
            Debug.Log("Radio should switch");
        }
    }
}
