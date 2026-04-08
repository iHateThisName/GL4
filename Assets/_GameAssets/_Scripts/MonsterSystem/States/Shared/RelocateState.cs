using UnityEngine;

namespace MonsterSystem
{
    /// <summary>
    /// Teleports Monster to a random position from the Transform config,
    /// avoiding the same position twice in a row, then transitions to the next state.
    /// </summary>
    public class RelocateState : MonsterState
    {
        [SerializeField] private MonsterState nextState; // State to transition to after relocation is complete
        [SerializeField] private SO_TransformCollection transforms;
        [SerializeField] private bool useConfig = true;
        [SerializeField] private bool killMomentum = false;

        private Rigidbody rb;
        private int lastIndex = -1; // Index of the last used spawn point to avoid repeats

        public override void Initialize(MonsterController owningController)
        {
            base.Initialize(owningController);
            this.rb = this.controller.GetComponent<Rigidbody>();
        }

        /// <summary>
        /// On entering this state, picks a random spawn point (different from the last one),
        /// teleports the monster there, and immediately transitions to the next state.
        /// </summary>
        public override void OnStateEnter()
        {
            if (this.useConfig)
            {
                // Retrieve the spawn points from the monster's shared config
                var spawnPoints = this.controller.SpawnPoints;
                if (spawnPoints != null && spawnPoints.points != null && spawnPoints.points.Length > 0)
                {
                    int index;

                    // If only one spawn point exists, use it directly
                    if (spawnPoints.points.Length == 1) index = 0;
                    else
                    {
                        // Pick a random index that differs from the last used index
                        do { index = Random.Range(0, spawnPoints.points.Length); }
                        while (index == this.lastIndex);
                    }

                    // Remember this index to avoid choosing it again next time
                    this.lastIndex = index;

                    // Teleport the monster to the selected spawn point's position and rotation
                    var point = spawnPoints.points[index];
                    this.controller.transform.SetPositionAndRotation(point.position, Quaternion.Euler(point.rotation));
                }
            }
            else
            {
                if(this.transforms != null && this.transforms.points.Length > 0)
                {
                    int index = Random.Range(0, this.transforms.points.Length);
                    Vector3 position = this.transforms.points[index].position;
                    Vector3 rotation = this.transforms.points[index].rotation;
                    this.controller.transform.SetPositionAndRotation(position, Quaternion.Euler(rotation));
                }
            }

            if (this.killMomentum)
                KillForces();
            
            // Immediately transition to the configured next state
            if (this.nextState != null)
                RequestTransition(this.nextState);
        }
        
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
