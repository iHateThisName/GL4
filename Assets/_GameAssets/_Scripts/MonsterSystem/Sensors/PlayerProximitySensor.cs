using System;
using UnityEngine;

namespace MonsterSystem
{
    [RequireComponent(typeof(SphereCollider))]
    public class PlayerProximitySensor : MonsterSensor
    {
        [SerializeField] private string playerTag = "Player";

        public bool IsPlayerInRange { get; private set; }
        public float DistanceToPlayer { get; private set; } = float.MaxValue;
        public Transform PlayerTransform { get; private set; }

        public event Action OnPlayerEntered;
        public event Action OnPlayerExited;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(playerTag)) return;

            IsPlayerInRange = true;
            PlayerTransform = other.transform;
            OnPlayerEntered?.Invoke();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag(playerTag)) return;

            IsPlayerInRange = false;
            PlayerTransform = null;
            DistanceToPlayer = float.MaxValue;
            OnPlayerExited?.Invoke();
        }

        public override void Tick(MonsterController controller)
        {
            if (IsPlayerInRange && PlayerTransform != null)
            {
                DistanceToPlayer = Vector3.Distance(transform.position, PlayerTransform.position);
            }
        }
    }
}
