using UnityEngine;

namespace MonsterSystem
{
    public class GazeSensor : MonsterSensor
    {
        [SerializeField] private float gazeAngleThreshold = 30f;
        [Tooltip("Optional override. Defaults to Camera.main if unset.")]
        [SerializeField] private Transform cameraOverride;

        public bool IsBeingObserved { get; private set; }

        private Transform cachedCamera;

        private Transform GetCamera()
        {
            if (cameraOverride != null) return cameraOverride;
            if (cachedCamera == null && Camera.main != null)
                cachedCamera = Camera.main.transform;
            return cachedCamera;
        }

        public override void OnTick(float tickDelta)
        {
            base.OnTick(tickDelta);

            Transform cam = GetCamera();
            if (cam == null)
            {
                IsBeingObserved = false;
                return;
            }

            Vector3 dirToMonster = (transform.position - cam.position).normalized;
            float dot = Vector3.Dot(cam.forward, dirToMonster);
            float angle = Mathf.Acos(Mathf.Clamp(dot, -1f, 1f)) * Mathf.Rad2Deg;

            IsBeingObserved = angle <= gazeAngleThreshold;
        }
    }
}
