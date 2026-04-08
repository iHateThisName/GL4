using UnityEngine;

namespace MonsterSystem
{
    /// <summary>
    /// Configuration asset that defines a set of spawn/navigation points
    /// used by NavMesh-based monster movement.
    /// </summary>
    [CreateAssetMenu(menuName = "Monsters/NavMesh Move Config")]
    public class SO_NavMeshMoveConfig : ScriptableObject
    {
        [Header("Points")]
        // Array of predefined spawn points the monster can navigate between
        public SpawnPoint[] points;
        
        /// <summary>
        /// Returns a randomly selected spawn point from the configured array,
        /// or a default (zero) spawn point if none are configured.
        /// </summary>
        /// <returns>A randomly chosen <see cref="SpawnPoint"/>, or default if the array is empty or null.</returns>
        public SpawnPoint GetRandom()
        {
            // Return default if no spawn points have been configured
            if (this.points == null || this.points.Length == 0)
                return default;

            // Select a random index from the available spawn points
            return this.points[Random.Range(0, this.points.Length)];
        }
    }
}
