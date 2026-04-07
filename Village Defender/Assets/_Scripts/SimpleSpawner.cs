using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SimpleSpawner : MonoBehaviour
{
    public List<WaveData> waves;
    public Transform spawnPoint;

    private Coroutine currentSpawnRoutine;

    public void StartCurrentWave(int waveIndex)
    {
        if (currentSpawnRoutine != null)
        {
            StopCoroutine(currentSpawnRoutine);
        }

        if (waves == null || waves.Count == 0)
        {
            Debug.LogError("Aucune vague assignée dans le SimpleSpawner.");
            GameManager.Instance.NotifySpawningFinished();
            return;
        }

        if (waveIndex < 0 || waveIndex >= waves.Count)
        {
            Debug.LogWarning("Il n'y a plus de vague disponible. Index demandé : " + waveIndex);
            GameManager.Instance.NotifySpawningFinished();
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("Aucun spawnPoint assigné dans le SimpleSpawner.");
            GameManager.Instance.NotifySpawningFinished();
            return;
        }

        currentSpawnRoutine = StartCoroutine(SpawnWave(waves[waveIndex]));
    }

    IEnumerator SpawnWave(WaveData wave)
    {
        if (wave == null)
        {
            Debug.LogError("La WaveData est null.");
            GameManager.Instance.NotifySpawningFinished();
            yield break;
        }

        foreach (WaveData.EnemyGroup group in wave.groups)
        {
            if (group == null || group.enemyType == null)
            {
                Debug.LogWarning("Un groupe d'ennemis est mal configuré dans la vague.");
                continue;
            }

            for (int i = 0; i < group.count; i++)
            {
                SpawnEnemy(group.enemyType);

                float delay = 1f;
                if (group.rate > 0f)
                    delay = 1f / group.rate;

                yield return new WaitForSeconds(delay);
            }
        }

        currentSpawnRoutine = null;
        GameManager.Instance.NotifySpawningFinished();
    }

    void SpawnEnemy(EnemyData data)
    {
        if (data == null)
        {
            Debug.LogError("EnemyData null dans SpawnEnemy.");
            return;
        }

        if (data.enemyPrefab == null)
        {
            Debug.LogError("Le prefab d'ennemi est null pour : " + data.name);
            return;
        }

        GameObject newEnemy = Instantiate(data.enemyPrefab, spawnPoint.position, spawnPoint.rotation);

        EnemyMovement script = newEnemy.GetComponent<EnemyMovement>();
        if (script != null)
        {
            script.data = data;
        }
        else
        {
            Debug.LogError("Le prefab ennemi n'a pas de script EnemyMovement.");
        }

        GameManager.Instance.RegisterEnemy();
    }
}