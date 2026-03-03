using UnityEngine;

namespace Refactored
{
    public class MonsterSpawner : MonoBehaviour
    {
        [Header("TEMP spawn points")]
        [SerializeField] private Transform[] munchSpawnPoints;
        [SerializeField] private Transform[] StalkerSpawnPoints;
        
        [SerializeField] private NightSettings nightSettings;
        
        private void OnEnable()
        {
            GameManager.OnEventAvailable += SpawnMonster;
        }

        private void OnDisable()
        {
            GameManager.OnEventAvailable -= SpawnMonster;
        }

        private void SpawnMonster(GameManager.NightEvent evt)
        {
            var eventData = evt.GetPayload();
            if (eventData.GetEventType() != GameManager.EventType.SpawnMonster) return;

            for (int i = 0; i < eventData.GetMonsterCount(); i++)
            {
                // GetPosition from Monster's Config if using refactored system, for now using Temporary Predetermined Positions.
                var monster = eventData.GetMonsterPrefab();
                if (monster == null) return;

                var stalkerType = TryGetComponent<BaseNavAIMonster>(out var stalker);
                var munchType = TryGetComponent<TheMunch>(out var munch);
                Vector3 spawnPosition = Vector3.zero;

                // is Stalker
                if (stalker && !munch)
                {
                    spawnPosition = StalkerSpawnPoints[Random.Range(0, StalkerSpawnPoints.Length)].position;
                }
                else if (munch && !stalker)
                {
                    spawnPosition = munchSpawnPoints[Random.Range(0, munchSpawnPoints.Length)].position;
                }
                
                Instantiate(monster, spawnPosition, Quaternion.identity);
            }
        }
    }
}