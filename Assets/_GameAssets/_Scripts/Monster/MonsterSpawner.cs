using Assets.Scripts.Singleton;
using MonsterSystem;
using UnityEngine;
using Random = UnityEngine.Random;

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

        private void SpawnMonsterWithEvent(GameManager.NightEvent evt)
        {
            var eventData = evt.GetPayload();
            if (eventData.GetEventType() != GameManager.EventType.SpawnMonster)
            {
                Debug.LogError("Event is not a SpawnMonster event.");
                return;
            }
            Debug.Log("Spawning Monster");

            for (int i = 0; i < eventData.GetMonsterCount(); i++)
            {
                Debug.Log("Spawning Monster in loop");
                var monster = eventData.GetMonsterPrefab();
                if (monster == null)
                {
                    Debug.LogError("Monster Prefab is null, cannot spawn monster.");
                    return;
                }
                
                var monsterController = monster.GetComponent<MonsterController>();
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
            var config = monsterController.Config;
            if (config == null || config.spawnPoints == null || config.spawnPoints.Length == 0)
            {
                Debug.LogError("Monster Config or spawn points missing, cannot spawn monster.");
                return;
            }

            var spawnPoint = config.GetRandomSpawnPoint();
            Instantiate(monsterToSpawn, spawnPoint.position, Quaternion.Euler(spawnPoint.rotation));
        }
    }
}