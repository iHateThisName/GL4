using Assets.Scripts.Singleton;
using MonsterSystem;
using UnityEngine;

namespace Refactored
{
    public class MonsterSpawner : Singleton<MonsterSpawner>
    {
        [Header("=== References ===")]
        [SerializeField] private SO_NightSettings nightSettings;
        
        private void OnEnable()
        {
            GameManager.OnEventAvailable += SpawnMonsterWithEvent;
        }

        private void OnDisable()
        {
            GameManager.OnEventAvailable -= SpawnMonsterWithEvent;
        }

        private void SpawnMonsterWithEvent(NightEvent evt)
        {
            if (evt.GetEventType() != NightEvent.NightEventType.SpawnMonster) return;

            for (int i = 0; i < evt.GetMonsterCount(); i++)
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

        private void SpawnMonster(GameObject monsterToSpawn, MonsterController monsterController)
        {
            var spawnPoints = monsterController.SpawnPoints;
            bool hasValidSpawnPoint = spawnPoints != null && spawnPoints.points != null && spawnPoints.points.Length > 0;
            
            var spawnPoint = hasValidSpawnPoint ? monsterController.SpawnPoints.GetRandom() : new SpawnPoint();
            Instantiate(monsterToSpawn, spawnPoint.position, Quaternion.Euler(spawnPoint.rotation));
        }
    }
}