using Assets.Scripts.Singleton;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Refactored
{
    public class MonsterSpawner : Singleton<MonsterSpawner>
    {
        [Header("TEMP spawn points")]
        [SerializeField] private Transform[] munchSpawnPoints;
        [SerializeField] private Transform[] StalkerSpawnPoints;
        
        [Header("Temp navigation points for spawning Stalker")]
        [SerializeField] private Transform[] stalkerPatrolPoints;
        
        [SerializeField] private NightSettings nightSettings;
        
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

                var stalker = monster.GetComponentInChildren<BaseNavAIMonster>();//.TryGetComponent<BaseNavAIMonster>(out var stalker); 
                var munch = monster.GetComponentInChildren<TheMunch>();;//.TryGetComponent<TheMunch>(out var munch);
                
                Debug.Log($"Monster: {monster.name}, Stalker: {stalker != null}, Munch: {munch != null}");
                
                BaseNavAIMonster.MonsterTypeEnum monsterType = BaseNavAIMonster.MonsterTypeEnum.None;

                // is Stalker
                if (stalker && !munch)
                {
                    monsterType = BaseNavAIMonster.MonsterTypeEnum.Stalker;
                }
                else if (munch && !stalker)
                {
                    monsterType = BaseNavAIMonster.MonsterTypeEnum.Munch;
                }
                
                SpawnMonster(monster, monsterType);
            }
        }

        private void SpawnMonster(GameObject monsterToSpawn, BaseNavAIMonster.MonsterTypeEnum monsterType)
        {
            Debug.Log($"Spawning Monster FR: {monsterToSpawn != null}, Type: {monsterType}");
            if (monsterType == BaseNavAIMonster.MonsterTypeEnum.None)
            {
                Debug.LogWarning("Monster Type is None, cannot spawn monster.");
                return;
            }
            
            // GetPosition from Monster's Config if using refactored system, for now using Temporary Predetermined Positions.
            Vector3 monsterSpawnPosition = GetMonsterSpawnPosition(monsterType);
            Quaternion monsterSpawnRotation = GetMonsterSpawnRotation(monsterType);
            
            Transform monsterSpawnPoint = GetMonsterSpawnPoint(monsterType);
            if (monsterSpawnPoint == null)
            {
                Debug.Log("Monster Spawn Point Not Found");
                return;
            }
            var monster = Instantiate(monsterToSpawn, monsterSpawnPoint.position, monsterSpawnPoint.rotation);//Instantiate(monsterToSpawn, monsterSpawnPosition, monsterSpawnRotation);
            Debug.Log($"Monster Euler Rotation: {monster.transform.eulerAngles}");
            Debug.Log($"Spawn Euler Rotation: {monsterSpawnPoint.rotation.eulerAngles}");
            if (monsterType == BaseNavAIMonster.MonsterTypeEnum.Stalker)
            {
                var navigationComponent = monster.GetComponentInChildren<BaseNavAIMonster>();
                if (navigationComponent == null) return;
                navigationComponent.SetPatrolPoints(stalkerPatrolPoints);
            }
        }

        public void RelocateMonster(Transform monsterTransform, BaseNavAIMonster.MonsterTypeEnum monsterType)
        {
            if (monsterTransform == null) return;
            
            Transform newPosition = GetMonsterSpawnPoint(monsterType);
            if (newPosition == null) return;
            
            monsterTransform.SetPositionAndRotation(newPosition.position, newPosition.rotation);
        }

        private Transform GetMonsterSpawnPoint(BaseNavAIMonster.MonsterTypeEnum monsterType)
        {
            switch (monsterType)
            {
                case BaseNavAIMonster.MonsterTypeEnum.None:
                    return null;
                    break;
                case BaseNavAIMonster.MonsterTypeEnum.Stalker:
                    return StalkerSpawnPoints[Random.Range(0, StalkerSpawnPoints.Length)];
                    break;
                case BaseNavAIMonster.MonsterTypeEnum.Munch:
                    return munchSpawnPoints[Random.Range(0, munchSpawnPoints.Length)];
                    break;
                default:
                    return null;
                    break;
            }
        }

        private Vector3 GetMonsterSpawnPosition(BaseNavAIMonster.MonsterTypeEnum monsterType)
        {
            switch (monsterType)
            {
                case BaseNavAIMonster.MonsterTypeEnum.None:
                    return Vector3.zero;
                    break;
                case BaseNavAIMonster.MonsterTypeEnum.Stalker:
                    return StalkerSpawnPoints[Random.Range(0, StalkerSpawnPoints.Length)].position;
                    break;
                case BaseNavAIMonster.MonsterTypeEnum.Munch:
                    return munchSpawnPoints[Random.Range(0, munchSpawnPoints.Length)].position;
                    break;
                default:
                    return Vector3.zero;
                    break;
            }
        }
        
        private Quaternion GetMonsterSpawnRotation(BaseNavAIMonster.MonsterTypeEnum monsterType)
        {
            switch (monsterType)
            {
                case BaseNavAIMonster.MonsterTypeEnum.None:
                    return Quaternion.identity;
                    break;
                case BaseNavAIMonster.MonsterTypeEnum.Stalker:
                    return StalkerSpawnPoints[Random.Range(0, StalkerSpawnPoints.Length)].rotation;
                    break;
                case BaseNavAIMonster.MonsterTypeEnum.Munch:
                    return munchSpawnPoints[Random.Range(0, munchSpawnPoints.Length)].rotation;
                    break;
                default:
                    return Quaternion.identity;
                    break;
            }
        }
    }
}