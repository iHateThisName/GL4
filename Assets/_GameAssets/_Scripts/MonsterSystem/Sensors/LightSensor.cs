using UnityEngine;

namespace MonsterSystem
{
    public class LightSensor : MonsterSensor
    {
        [Header("=== References ===")]
        [SerializeField] private LayerMask occlusionMask;
        [SerializeField] private SO_FlashlightSettings flashlightSettings;

        [Header("=== Flee State ===")]
        [SerializeField] private MonsterState fleeState;

        [Header("=== Configuration ===")]
        [SerializeField] private float exposureBuildSpeed = 3f;
        [SerializeField] private float exposureDecaySpeed = 2f;
        [SerializeField] private float stunThreshold = 1f;
        [SerializeField] private float sensorCooldownDuration = 5f;

        [Header("=== Audio ===")]
        [SerializeField] private AudioClip flashedSound;

        private DetectionConeData cachedCone;
        private Transform sensorTransform;
        private float remainingCooldownTime;
        private float exposure;
        private Transform FlashlightTransform => this.flashlightSettings != null ? this.flashlightSettings.FlashlightTransform : null;

        public override void Initialize(MonsterController owningMonster)
        {
            base.Initialize(owningMonster);
            this.sensorTransform = owningMonster.transform;

            if (this.flashlightSettings != null)
            {
                this.flashlightSettings.OnRuntimeDataChanged += RefreshCachedData;
                RefreshCachedData();
            }
        }

        private void OnDestroy()
        {
            if (this.flashlightSettings != null)
                this.flashlightSettings.OnRuntimeDataChanged -= RefreshCachedData;
        }

        private void RefreshCachedData()
        {
            this.cachedCone = this.flashlightSettings.DetectionCone;
        }

        public override void OnTick(float tickDelta)
        {
            base.OnTick(tickDelta);

            // Handle cooldown - skip all processing during stun immunity
            if (this.remainingCooldownTime > 0)
            {
                this.remainingCooldownTime -= tickDelta;
                return;
            }

            // No valid flashlight or flashlight is off
            if (FlashlightTransform == null)
            {
                AdjustExposure(-this.exposureDecaySpeed);
                return;
            }

            Vector3 flashLightPos = FlashlightTransform.position;
            Vector3 sensorPos = this.sensorTransform.position;
            Vector3 toSensor = sensorPos - flashLightPos;
            float distanceSquared = toSensor.sqrMagnitude;

            // Sensor is outside flashlight range
            if (distanceSquared > this.cachedCone.RangeSquared)
            {
                AdjustExposure(-this.exposureDecaySpeed);
                return;
            }

            // Flatten to XZ plane (ignore vertical angle)
            Vector3 flashlightForward = FlashlightTransform.forward;
            flashlightForward.y = 0f;
            flashlightForward.Normalize();

            Vector3 toSensorFlat = toSensor;
            toSensorFlat.y = 0f;
            float flatDistanceSquared = toSensorFlat.sqrMagnitude;

            float rawDot = Vector3.Dot(flashlightForward, toSensorFlat);
            // Sensor is behind the flashlight (horizontally)
            if (rawDot <= 0f)
            {
                AdjustExposure(-this.exposureDecaySpeed);
                return;
            }

            // Sensor is outside the flashlight cone angle (horizontal only)
            // Uses squared comparison to avoid sqrt: (dot)^2 < (cosThreshold)^2 * dist^2
            if (rawDot * rawDot < this.cachedCone.CosineThresholdSquared * flatDistanceSquared)
            {
                AdjustExposure(-this.exposureDecaySpeed);
                return;
            }

            // Sensor is occluded by geometry (wall, obstacle, etc.)
            if (Physics.Linecast(flashLightPos, sensorPos, this.occlusionMask))
            {
                AdjustExposure(-this.exposureDecaySpeed);
                return;
            }

            // Sensor is in the light - calculate exposure intensity based on cone position (horizontal)
            // Intensity is higher when closer to the center of the cone
            float flatDistance = Mathf.Sqrt(flatDistanceSquared);
            float dot = rawDot / flatDistance;
            float intensity = (dot - this.cachedCone.CosineThreshold) * this.cachedCone.InverseConeRange;

            // Build exposure based on intensity
            AdjustExposure(intensity * this.exposureBuildSpeed);

            // Check for stun threshold
            if (this.exposure >= this.stunThreshold)
            {
                Stun();
            }
        }

        /// <summary>
        /// Clamps exposure to new value over time using TickDelta from base class.
        /// </summary>
        private void AdjustExposure(float rate)
        {
            this.exposure = Mathf.Clamp01(this.exposure + rate * TickDelta);
        }

        /// <summary>
        /// Triggers the stun effect on the monster and starts the cooldown period.
        /// Resets exposure to zero to prevent immediate re-stun after cooldown.
        /// Transitions to the flee state, passing the flashlight as the target to flee from.
        /// </summary>
        private void Stun()
        {
            this.remainingCooldownTime = this.sensorCooldownDuration;
            this.exposure = 0f;

            // Play stun audio
            if (this.flashedSound != null && this.controller.Audio != null)
                MonsterAudio.PlayOneShot(this.controller.Audio, this.flashedSound);

            // Transition to flee state with flashlight as target (NavMeshMoveState.AwayFromTarget mode)
            if (this.fleeState != null && FlashlightTransform != null)
                TriggerTransitionTo(this.fleeState, FlashlightTransform);
        }

        /// <summary>                                                                                                                                                            
        /// Draws a debug gizmo showing current exposure level.                                                                                                                  
        /// Color transitions from green (no exposure) to red (full exposure).                                                                                                   
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.Lerp(Color.green, Color.red, this.exposure);
            Gizmos.DrawWireSphere(this.transform.position, 0.15f);
        }
    }
}
