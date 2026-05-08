using Assets.Scripts.Singleton;
using MonsterSystem;
using UnityEngine;

namespace Refactored
{
    /// <summary>
    /// Singleton that listens for <see cref="NightEvent.NightEventType.SpawnMonster"/> night events
    /// and instantiates the configured monster prefab at a randomly selected spawn point.
    /// Respects the monster's own <see cref="MonsterController.SpawnPoints"/> collection; falls back
    /// to the origin if no valid spawn points are defined.
    /// </summary>
    public class MonsterSpawner : Singleton<MonsterSpawner>
    {
        [Header("=== References ===")]
        [SerializeField] private SO_NightSettings nightSettings;
        
        /// <summary>
        /// Subscribes to the night-event stream so spawn requests are handled automatically.
        /// </summary>
        private void OnEnable()
        {
            GameManager.OnEventAvailable += SpawnMonsterWithEvent;
        }

        /// <summary>
        /// Unsubscribes from the night-event stream to prevent callbacks after destruction.
        /// </summary>
        private void OnDisable()
        {
            GameManager.OnEventAvailable -= SpawnMonsterWithEvent;
        }

        /// <summary>
        /// Handles an incoming night event. Ignores events that are not of type
        /// <see cref="NightEvent.NightEventType.SpawnMonster"/>; otherwise spawns the
        /// configured number of monsters defined by the event.
        /// </summary>
        /// <param name="evt">The night event dispatched by <see cref="GameManager"/>.</param>
        private void SpawnMonsterWithEvent(NightEvent evt)
        {
            if (evt.GetEventType() != NightEvent.NightEventType.SpawnMonster) return;

            for (int i = 0; i < evt.GetAmount(); i++)
            {
                Debug.Log("Spawning Monster in loop");
                var monster = evt.GetMonsterPrefab();
                if (monster == null)
                {
                    Debug.LogError("Monster Prefab is null, cannot spawn monster.");
                    return;
                }

                var monsterController = monster.GetComponentInChildren<MonsterController>();
                if (monsterController == null)
                {
                    Debug.LogError("Monster Controller is null, cannot spawn monster.");
                    return;
                }
                SpawnMonster(monster, monsterController);
            }
        }

        /// <summary>
        /// Selects a random spawn point from the monster controller's collection and instantiates
        /// the monster prefab at that position and rotation. Falls back to the world origin if no
        /// spawn points are configured.
        /// </summary>
        /// <param name="monsterToSpawn">The prefab to instantiate.</param>
        /// <param name="monsterController">Controller used to retrieve the spawn-point collection.</param>
        private void SpawnMonster(GameObject monsterToSpawn, MonsterController monsterController)
        {
            var spawnPoints = monsterController.SpawnPoints;
            bool hasValidSpawnPoint = spawnPoints != null && spawnPoints.points != null && spawnPoints.points.Length > 0;
            
            var spawnPoint = hasValidSpawnPoint ? monsterController.SpawnPoints.GetRandom() : new SpawnPoint();
            Instantiate(monsterToSpawn, spawnPoint.position, Quaternion.Euler(spawnPoint.rotation));
        }
    }
}