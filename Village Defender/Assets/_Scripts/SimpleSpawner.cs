using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SimpleSpawner : MonoBehaviour
{
    public List<WaveData> waves;
    public Transform spawnPoint;

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        foreach (WaveData wave in waves)
        {
            yield return new WaitUntil(() => GameManager.RunWave == true);
            foreach (WaveData.EnemyGroup group in wave.groups)
            {
                for (int i = 0; i < group.count; i++)
                { 
                    SpawnEnemy(group.enemyType); 
                    yield return new WaitForSeconds(1f / group.rate);
                }
            }
            GameManager.RunWave = false;
        }
    }

    void SpawnEnemy(EnemyData data)
    {
        if (data.enemyPrefab == null) return;
        
        GameObject newEnemy = Instantiate(data.enemyPrefab, spawnPoint.position, spawnPoint.rotation);

        EnemyMovement script = newEnemy.GetComponent<EnemyMovement>(); 
        script.data = data;
        
        GameManager.Instance.RegisterEnemy();
    }
}