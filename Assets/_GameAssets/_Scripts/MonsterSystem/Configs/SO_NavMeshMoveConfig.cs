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
        public MonsterConfig.SpawnPoint[] points;
    }
}
