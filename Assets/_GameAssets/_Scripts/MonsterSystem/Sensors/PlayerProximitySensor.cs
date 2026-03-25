using System;
using UnityEngine;

namespace MonsterSystem
{
    /// <summary>
    /// Sensor that monitors the distance between the monster and the player,
    /// raising events when the player enters or exits the detection range.
    /// </summary>
    public class PlayerProximitySensor : MonsterSensor
    {
        [SerializeField] private float detectionRange = 10f;
        [SerializeField] private MonsterState impatient;
        [SerializeField] private MonsterState resetPatience;

        private float detectionRangeSqr;
        public bool IsPlayerInRange { get; private set; }

        /// <summary>
        /// Squared distance to player. Use for comparisons to avoid sqrt.
        /// </summary>
        public float SqrDistanceToPlayer { get; private set; } = float.MaxValue;

        public Transform PlayerTransform => this.controller.Config.PlayerTarget;

        public override void Initialize(MonsterController owningMonster)
        {
            base.Initialize(owningMonster);
            this.detectionRangeSqr = this.detectionRange * this.detectionRange;
        }

        public override void OnTick(float tickDelta)
        {
            base.OnTick(tickDelta);

            Transform player = this.PlayerTransform;
            if (player == null) return;

            // sqrMagnitude avoids sqrt — significant on ARM/Quest 2
            this.SqrDistanceToPlayer = (this.transform.position - player.position).sqrMagnitude;
            this.IsPlayerInRange = this.SqrDistanceToPlayer <= this.detectionRangeSqr;

            if (this.IsPlayerInRange)
                TriggerStateTransition(resetPatience);
        }
    }
}
