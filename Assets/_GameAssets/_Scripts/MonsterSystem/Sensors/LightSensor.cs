using System;
using UnityEngine;

namespace MonsterSystem
{
    public class LightSensor : MonsterSensor
    {
        [SerializeField] private string lightTag = "Flashlight";

        public bool IsLitUp { get; private set; }

        public event Action OnLightDetected;
        public event Action OnLightLost;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(lightTag)) return;

            IsLitUp = true;
            OnLightDetected?.Invoke();
        }

        private void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag(lightTag)) return;

            IsLitUp = false;
            OnLightLost?.Invoke();
        }
    }
}
