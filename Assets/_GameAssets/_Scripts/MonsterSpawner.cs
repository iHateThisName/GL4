using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    /* =======================
     * Serialized Fields
     * ======================= */

    [Header("Spawn Settings")]
    [SerializeField] private GameObject[] monsterPrefabs;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float spawnInterval = 3f;

    /* =======================
     * Private Fields
     * ======================= */

    private GameObject currentMonster;

    /* =======================
     * Unity Lifecycle
     * ======================= */

    private void OnEnable()
    {
        NightCycle.OnEventAvailable += SpawnMonster;
    }

    private void OnDisable()
    {
        NightCycle.OnEventAvailable -= SpawnMonster;
    }

    /* =======================
     * Spawning Logic
     * ======================= */

    private void SpawnMonster()
    {
        // ✅ Only allow one monster at a time
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

    /* =======================
     * Helpers
     * ======================= */

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
