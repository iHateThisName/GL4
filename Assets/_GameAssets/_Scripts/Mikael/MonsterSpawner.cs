using System.Collections;
using UnityEngine;

public class MonsterSpawner : MonoBehaviour
{
    /* =======================
     * Serialized Fields
     * ======================= */

    [Header("Spawn Settings")]
    [Tooltip("Monster prefabs that can be spawned")]
    [SerializeField] public GameObject[] MonsterPrefabs;

    [Tooltip("Possible spawn locations")]
    [SerializeField] public Transform[] SpawnPoints;

    [Tooltip("Time in seconds between spawns")]
    [SerializeField] public float SpawnInterval = 3f;

    [Tooltip("Maximum number of monsters to spawn (0 = infinite)")]
    [SerializeField] public int MaxMonsters = 10;


    /* =======================
     * Private Fields
     * ======================= */

    private int currentMonsterCount;


    /* =======================
     * Unity Lifecycle
     * ======================= */

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }


    /* =======================
     * Spawning Logic
     * ======================= */

    private IEnumerator SpawnLoop()
    {
        while (CanSpawnMoreMonsters())
        {
            SpawnMonster();
            yield return new WaitForSeconds(this.SpawnInterval);
        }
    }

    private bool CanSpawnMoreMonsters()
    {
        return this.MaxMonsters == 0 || this.currentMonsterCount < this.MaxMonsters;
    }

    private void SpawnMonster()
    {
        if (!IsSpawnerConfiguredCorrectly())
        {
            Debug.LogWarning("MonsterSpawner is missing prefabs or spawn points.");
            return;
        }

        GameObject selectedPrefab = GetRandomMonsterPrefab();
        Transform selectedSpawnPoint = GetRandomSpawnPoint();

        Instantiate(
            selectedPrefab,
            selectedSpawnPoint.position,
            selectedSpawnPoint.rotation
        );

        this.currentMonsterCount++;
    }


    /* =======================
     * Helpers
     * ======================= */

    private bool IsSpawnerConfiguredCorrectly()
    {
        return this.MonsterPrefabs.Length > 0 && this.SpawnPoints.Length > 0;
    }

    private GameObject GetRandomMonsterPrefab()
    {
        int index = Random.Range(0, this.MonsterPrefabs.Length);
        return this.MonsterPrefabs[index];
    }

    private Transform GetRandomSpawnPoint()
    {
        int index = Random.Range(0, this.SpawnPoints.Length);
        return this.SpawnPoints[index];
    }
}
