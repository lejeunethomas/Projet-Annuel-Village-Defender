using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SimpleSpawner : MonoBehaviour
{
    [Header("Vagues")]
    public List<WaveData> waves;

    [Header("Points de spawn")]
    public List<Transform> spawnPoints = new List<Transform>();

    private Coroutine _currentSpawnRoutine;
    private int _nextSpawnPointIndex = 0;
    private readonly List<GameObject> spawnedEnemies = new List<GameObject>();
    private bool waveWasCancelled;

    public void StartCurrentWave(int waveIndex)
    {
        if (_currentSpawnRoutine != null)
        {
            StopCoroutine(_currentSpawnRoutine);
            _currentSpawnRoutine = null;
        }

        RemoveNullSpawnedEnemies();
        waveWasCancelled = false;

        if (waves == null || waves.Count == 0)
        {
            Debug.LogError("Aucune vague assignée dans le SimpleSpawner.");
            NotifySpawningFinishedIfCurrentWaveActive();
            return;
        }

        if (waveIndex < 0 || waveIndex >= waves.Count)
        {
            Debug.LogWarning("SimpleSpawner : aucune WaveData assignée pour l'index demandé " + waveIndex + ". Vérifiez la liste ordonnée des vagues dans l'Inspector.");
            NotifySpawningFinishedIfCurrentWaveActive();
            return;
        }

        if (waves[waveIndex] == null)
        {
            Debug.LogError("SimpleSpawner : WaveData manquante à l'index " + waveIndex + " dans la liste ordonnée des vagues.");
            NotifySpawningFinishedIfCurrentWaveActive();
            return;
        }

        if (spawnPoints == null || spawnPoints.Count == 0)
        {
            Debug.LogError("Aucun point de spawn assigné dans le SimpleSpawner.");
            NotifySpawningFinishedIfCurrentWaveActive();
            return;
        }

        for (int i = spawnPoints.Count - 1; i >= 0; i--)
        {
            if (spawnPoints[i] == null)
            {
                spawnPoints.RemoveAt(i);
            }
        }

        if (spawnPoints.Count == 0)
        {
            Debug.LogError("Tous les points de spawn du SimpleSpawner sont null.");
            NotifySpawningFinishedIfCurrentWaveActive();
            return;
        }

        _nextSpawnPointIndex = 0;
        _currentSpawnRoutine = StartCoroutine(SpawnWave(waves[waveIndex]));
    }

    public void StopCurrentWaveAndDestroyEnemies()
    {
        waveWasCancelled = true;

        if (_currentSpawnRoutine != null)
        {
            StopCoroutine(_currentSpawnRoutine);
            _currentSpawnRoutine = null;
        }

        for (int i = 0; i < spawnedEnemies.Count; i++)
        {
            if (spawnedEnemies[i] != null)
                Destroy(spawnedEnemies[i]);
        }

        spawnedEnemies.Clear();
        _nextSpawnPointIndex = 0;
    }

    IEnumerator SpawnWave(WaveData wave)
    {
        if (wave == null)
        {
            Debug.LogError("La WaveData est null.");
            NotifySpawningFinishedIfCurrentWaveActive();
            yield break;
        }

        foreach (WaveData.EnemyGroup group in wave.groups)
        {
            if (waveWasCancelled)
                yield break;

            if (group == null || group.enemyType == null)
            {
                Debug.LogWarning("Un groupe d'ennemis est mal configuré dans la vague.");
                continue;
            }

            for (int i = 0; i < group.count; i++)
            {
                if (waveWasCancelled)
                    yield break;

                SpawnEnemy(group.enemyType, GetNextSpawnPoint());

                float delay = 1f;
                if (group.rate > 0f)
                    delay = 1f / group.rate;

                yield return new WaitForSeconds(delay);
            }
        }

        _currentSpawnRoutine = null;
        NotifySpawningFinishedIfCurrentWaveActive();
    }

    Transform GetNextSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Count == 0)
            return null;

        Transform point = spawnPoints[_nextSpawnPointIndex];
        _nextSpawnPointIndex = (_nextSpawnPointIndex + 1) % spawnPoints.Count;
        return point;
    }

    void SpawnEnemy(EnemyData data, Transform spawnPoint)
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

        if (spawnPoint == null)
        {
            Debug.LogError("SpawnPoint null dans SpawnEnemy.");
            return;
        }

        GameObject newEnemy = Instantiate(data.enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        spawnedEnemies.Add(newEnemy);

        EnemyMovement script = newEnemy.GetComponent<EnemyMovement>();
        if (script != null)
        {
            script.data = data;
        }
        else
        {
            Debug.LogError("Le prefab ennemi n'a pas de script EnemyMovement.");
        }

        if (GameManager.Instance != null)
            GameManager.Instance.RegisterEnemy();
    }

    private void RemoveNullSpawnedEnemies()
    {
        for (int i = spawnedEnemies.Count - 1; i >= 0; i--)
        {
            if (spawnedEnemies[i] == null)
                spawnedEnemies.RemoveAt(i);
        }
    }

    private void NotifySpawningFinishedIfCurrentWaveActive()
    {
        if (waveWasCancelled ||
            GameManager.Instance == null ||
            GameManager.Instance.CurrentPhase != GameManager.GamePhase.Wave)
        {
            return;
        }

        GameManager.Instance.NotifySpawningFinished();
    }
}
