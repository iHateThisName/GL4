using Assets.Scripts.Singleton;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Refactored
{
    public class MonsterSpawner : Singleton<MonsterSpawner>
    {
        [Header("TEMP spawn points")]
        [SerializeField] private Transform[] munchSpawnPoints;
        [SerializeField] private Transform[] stalkerSpawnPoints;
        
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
                
                var lightSensor = monster.GetComponentInChildren<LightSensor>();
                if (lightSensor != null) 
                    lightSensor.SetFlashLight(FlashLight.Instance);
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
            Transform spawnPoint = null;
            
            switch (monsterType)
            {
                case BaseNavAIMonster.MonsterTypeEnum.None:
                    spawnPoint = null;
                    break;
                case BaseNavAIMonster.MonsterTypeEnum.Stalker:
                    if (stalkerSpawnPoints == null || stalkerSpawnPoints.Length == 0)
                    {
                        Debug.LogError("No stalker spawn points assigned!");
                        return null;
                    }
                    spawnPoint = stalkerSpawnPoints[Random.Range(0, stalkerSpawnPoints.Length)];
                    break;
                case BaseNavAIMonster.MonsterTypeEnum.Munch:
                    if (munchSpawnPoints == null || munchSpawnPoints.Length == 0)
                    {
                        Debug.LogError("No munch spawn points assigned!");
                        return null;
                    }
                    spawnPoint = munchSpawnPoints[Random.Range(0, munchSpawnPoints.Length)];
                    break;
                default:
                    spawnPoint = null;
                    break;
            }
            return spawnPoint;
        }
    }
}