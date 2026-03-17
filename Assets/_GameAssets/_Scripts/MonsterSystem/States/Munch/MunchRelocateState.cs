using UnityEngine;

namespace MonsterSystem
{
    /// <summary>
    /// Teleports Munch to a random shadow/spawn position from the monster config,
    /// avoiding the same position twice in a row, then transitions to the next state.
    /// </summary>
    public class MunchRelocateState : MonsterState
    {
        [SerializeField] private MonsterState nextState; // State to transition to after relocation is complete

        private int lastIndex = -1; // Index of the last used spawn point to avoid repeats

        /// <summary>
        /// On entering this state, picks a random spawn point (different from the last one),
        /// teleports the monster there, and immediately transitions to the next state.
        /// </summary>
        public override void OnStateEnter()
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
        }
    }
}
