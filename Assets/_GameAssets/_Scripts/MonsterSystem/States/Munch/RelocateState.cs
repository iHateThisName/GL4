using System.Drawing;
using UnityEngine;

namespace MonsterSystem
{
    /// <summary>
    /// Teleports Munch to a random shadow/spawn position from the monster config,
    /// avoiding the same position twice in a row, then transitions to the next state.
    /// </summary>
    public class RelocateState : MonsterState
    {
        [SerializeField] private MonsterState nextState; // State to transition to after relocation is complete
        [SerializeField] private SO_NavMeshMoveConfig transforms;
        [SerializeField] private bool useConfig = true;

        private int lastIndex = -1; // Index of the last used spawn point to avoid repeats

        /// <summary>
        /// On entering this state, picks a random spawn point (different from the last one),
        /// teleports the monster there, and immediately transitions to the next state.
        /// </summary>
        public override void OnStateEnter()
        {
            if (this.useConfig)
            {
                // Retrieve the spawn points from the monster's shared config
                var config = this.controller.Config;
                if (config != null && config.spawnPoints != null && config.spawnPoints.Length > 0)
                {
                    int index;

                    // If only one spawn point exists, use it directly
                    if (config.spawnPoints.Length == 1)
                    {
                        index = 0;
                    }
                    else
                    {
                        // Pick a random index that differs from the last used index
                        do
                        {
                            index = Random.Range(0, config.spawnPoints.Length);
                        }
                        while (index == this.lastIndex);
                    }

                    // Remember this index to avoid choosing it again next time
                    this.lastIndex = index;

                    // Teleport the monster to the selected spawn point's position and rotation
                    var point = config.spawnPoints[index];
                    this.controller.transform.SetPositionAndRotation(point.position, Quaternion.Euler(point.rotation));
                }

                // Immediately transition to the configured next state
                if (this.nextState != null)
                    RequestTransition(this.nextState);
            } else
            {
                if(this.transforms != null && this.transforms.points.Length > 0)
                {
                    int index = Random.Range(0, this.transforms.points.Length);
                    Vector3 position = this.transforms.points[index].position;
                    Vector3 rotation = this.transforms.points[index].rotation;
                    this.controller.transform.SetPositionAndRotation(position, Quaternion.Euler(rotation));
                }
                if (this.nextState != null)
                    RequestTransition(this.nextState);
            }
        }
    }
}
