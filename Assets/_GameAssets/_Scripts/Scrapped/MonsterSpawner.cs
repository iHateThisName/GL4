using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private GameObject[] monsterPrefabs;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnInterval = 3f;

    private GameObject currentMonster;

    private void OnEnable()
    {
        GameManager.OnEventAvailable += SpawnMonster;
    }

    private void OnDisable()
    {
        GameManager.OnEventAvailable -= SpawnMonster;
    }

    
    private void SpawnMonster(NightEvent eventData)
    {
        if (currentMonster != null)
            return;

        if (!IsSpawnerConfiguredCorrectly())
        {
            Debug.LogWarning("MonsterSpawner is missing prefabs or spawn points.");
            return;
        }

        GameObject selectedPrefab = GetRandomMonsterPrefab();
        Transform selectedSpawnPoint = GetRandomSpawnPoint();

        currentMonster = Instantiate(
            selectedPrefab,
            selectedSpawnPoint.position,
            selectedSpawnPoint.rotation
        );
    }

    

    private bool IsSpawnerConfiguredCorrectly()
    {
        return monsterPrefabs.Length > 0 && spawnPoints.Length > 0;
    }

    private GameObject GetRandomMonsterPrefab()
    {
        return monsterPrefabs[Random.Range(0, monsterPrefabs.Length)];
    }

    private Transform GetRandomSpawnPoint()
    {
        return spawnPoints[Random.Range(0, spawnPoints.Length)];
    }
}
