using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaveManager : MonoBehaviour
{
    [Header("Enemy Settings")]
    public GameObject enemyPrefab;
    public int startingEnemies = 5;
    public int enemyIncreasePerWave = 2;

    [Header("Wave Settings")]
    public float timeBetweenWaves = 5f;

    [Header("Spawn Area")]
    public List<Transform> spawnPoints;

    private int currentWave = 1;
    private List<GameObject> currentEnemies = new List<GameObject>();

    private GameManager gm;

    void Start()
    {
        gm = GameManager.Instance;

        if (spawnPoints.Count == 0)
        {
            Debug.LogError("No spawn points assigned to WaveSpawner!");
            return;
        }

        // Show Wave 1 on the next frame after GameManager initializes
        StartCoroutine(ShowWave1NextFrame());

        // Spawn Wave 1 immediately
        SpawnWave(startingEnemies);

        StartCoroutine(StartWaveLoop());
    }

    IEnumerator ShowWave1NextFrame()
    {
        yield return null; // Wait one frame so GameManager.Start finishes
        gm.ShowWavePopup(1);
    }

    IEnumerator StartWaveLoop()
    {
        while (true)
        {
            // Wait for the wave to finish
            while (currentEnemies.Count > 0)
            {
                currentEnemies.RemoveAll(item => item == null);
                yield return null;
            }

            // Wait before next wave
            yield return new WaitForSecondsRealtime(timeBetweenWaves);

            currentWave++;

            // Show popup before spawning next wave
            gm.ShowWavePopup(currentWave);

            int enemiesToSpawn = startingEnemies + (currentWave - 1) * enemyIncreasePerWave;
            SpawnWave(enemiesToSpawn);
        }
    }

    void SpawnWave(int count)
    {
        currentEnemies.Clear();

        for (int i = 0; i < count; i++)
        {
            currentEnemies.Add(SpawnEnemy());
        }
    }

    GameObject SpawnEnemy()
    {
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Count)];
        GameObject enemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);

        EnemyAI ai = enemy.GetComponent<EnemyAI>();
        Transform player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (ai != null && player != null)
            ai.player = player;

        return enemy;
    }
}