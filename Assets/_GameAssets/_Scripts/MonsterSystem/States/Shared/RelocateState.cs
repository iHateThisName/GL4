using UnityEngine;

namespace MonsterSystem
{
    /// <summary>
    /// Teleports Munch to a random shadow/spawn position from the monster config,
    /// avoiding the same position twice in a row, then transitions to the next state.
    /// </summary>
    public class RelocateState : MonsterState
    {
        [SerializeField] private MonsterState nextState;
        [SerializeField] private SO_NavMeshMoveConfig transforms;
        [SerializeField] private bool useConfig = true;

        [Header("=== Physics ===")]
        [SerializeField] private Rigidbody rb; // Added Rigidbody reference

        private int lastIndex = -1;

        public override void OnStateEnter()
        {
            if (this.useConfig)
            {
                var config = this.controller.Config;
                if (config != null && config.spawnPoints != null && config.spawnPoints.Length > 0)
                {
                    int index;
                    if (config.spawnPoints.Length == 1)
                    {
                        index = 0;
                    }
                    else
                    {
                        do { index = Random.Range(0, config.spawnPoints.Length); }
                        while (index == this.lastIndex);
                    }

                    this.lastIndex = index;

                    var point = config.spawnPoints[index];
                    this.controller.transform.SetPositionAndRotation(point.position, Quaternion.Euler(point.rotation));

                    // Kill momentum
                    KillForces();
                }

                if (this.nextState != null) RequestTransition(this.nextState);
            }
            else
            {
                if (this.transforms != null && this.transforms.points.Length > 0)
                {
                    int index = Random.Range(0, this.transforms.points.Length);
                    Vector3 position = this.transforms.points[index].position;
                    Vector3 rotation = this.transforms.points[index].rotation;
                    this.controller.transform.SetPositionAndRotation(position, Quaternion.Euler(rotation));

                    // Kill momentum after teleporting so she doesn't fall out of her hiding spot
                    KillForces();
                }

                if (this.nextState != null) RequestTransition(this.nextState);
            }
        }

        // Helper method to keep code clean
        private void KillForces()
        {
            if (this.rb != null)
            {
                this.rb.linearVelocity = Vector3.zero;
                this.rb.angularVelocity = Vector3.zero;
            }
        }
    }
}