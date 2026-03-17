using UnityEngine;

namespace MonsterSystem
{
    /// <summary>
    /// Abstract base ScriptableObject that defines shared configuration for all monster types,
    /// including tick rate, spawn points, runtime references, and per-night difficulty scaling.
    /// </summary>
    public abstract class MonsterConfig : ScriptableObject
    {
        [Header("General")]
        public float baseTickRate = 0.2f; // Interval in seconds between monster AI ticks

        [Header("Runtime References")]
        [SerializeField] private SO_RuntimeReferences runtimeRefs; // Shared runtime reference container providing access to the player transform

        /// <summary>
        /// Gets the player's transform from runtime references, or null if unavailable.
        /// </summary>
        public Transform PlayerTarget => this.runtimeRefs != null ? this.runtimeRefs.Player : null;

        [Header("Spawn Points")]
        public SpawnPoint[] spawnPoints; // Array of predefined world-space spawn locations for this monster

        /// <summary>
        /// Represents a world-space spawn location defined by position and rotation.
        /// </summary>
        [System.Serializable]
        public struct SpawnPoint
        {
            public Vector3 position; // World-space position of the spawn point
            public Vector3 rotation; // Euler angles defining the spawn orientation
        }

        /// <summary>
        /// Returns a randomly selected spawn point from the configured array,
        /// or a default (zero) spawn point if none are configured.
        /// </summary>
        /// <returns>A randomly chosen <see cref="SpawnPoint"/>, or default if the array is empty or null.</returns>
        public SpawnPoint GetRandomSpawnPoint()
        {
            // Return default if no spawn points have been configured
            if (this.spawnPoints == null || this.spawnPoints.Length == 0)
                return default;

            // Select a random index from the available spawn points
            return this.spawnPoints[Random.Range(0, this.spawnPoints.Length)];
        }

        [Header("Night Scaling")]
        public NightOverride[] nightOverrides; // Per-night difficulty multipliers for scaling monster behaviour

        /// <summary>
        /// Defines per-night multipliers for patience, aggression, and speed,
        /// allowing the monster's difficulty to scale across successive nights.
        /// </summary>
        [System.Serializable]
        public struct NightOverride
        {
            public int nightNumber; // The night index this override applies to

            [Tooltip("1.0 = normal, 0.5 = half patience (harder)")]
            public float patienceMultiplier; // Scales how long the monster waits before acting; lower values increase difficulty

            [Tooltip("1.0 = normal, 2.0 = double aggression")]
            public float aggressionMultiplier; // Scales monster aggression; higher values increase difficulty

            [Tooltip("1.0 = normal speed")]
            public float speedMultiplier; // Scales monster movement speed; higher values make the monster faster
        }

        /// <summary>
        /// Searches the night overrides array for a matching night number and returns it.
        /// If no override is defined for the given night, returns a neutral override with all multipliers set to 1.
        /// </summary>
        /// <param name="night">The night number to look up.</param>
        /// <returns>The <see cref="NightOverride"/> for the specified night, or a default neutral override.</returns>
        public NightOverride GetOverrideForNight(int night)
        {
            // Search configured overrides for a matching night number
            if (this.nightOverrides != null)
            {
                for (int i = 0; i < this.nightOverrides.Length; i++)
                {
                    if (this.nightOverrides[i].nightNumber == night)
                        return this.nightOverrides[i];
                }
            }

            // No override found; return neutral multipliers (no scaling)
            return new NightOverride
            {
                nightNumber = night,
                patienceMultiplier = 1f,
                aggressionMultiplier = 1f,
                speedMultiplier = 1f
            };
        }
    }
}
