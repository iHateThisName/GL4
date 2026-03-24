using UnityEngine;

namespace MonsterSystem
{
    /// <summary>
    /// A sensor that listens for GameManager events and triggers state transitions.
    /// </summary>
    public class TouchSensor : MonsterSensor
    {
        [Header("Event Settings")]
        [Tooltip("The event type this sensor responds to")]
        [SerializeField] private TriggerArea touchArea;
        [SerializeField] private MonsterState patientState;

        protected override void Subscribe()
        {
            base.Subscribe();
            this.touchArea.OnTriggerEntered += OnPetted;
        }

        protected override void Unsubscribe()
        {
            base.Unsubscribe();
            this.touchArea.OnTriggerEntered -= OnPetted;
        }

        private void OnPetted(Collider other)
        {
            TriggerStateTransition(this.patientState);
        }
    }
}
