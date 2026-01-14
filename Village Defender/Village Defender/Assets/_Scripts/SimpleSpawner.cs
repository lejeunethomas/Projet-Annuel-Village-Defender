using UnityEngine;
using System.Collections;

public class SimpleSpawner : MonoBehaviour
{
    [Header("Configuration")]
    public EnemyData enemyData;
    public Transform spawnPoint;
    public float timeBetweenSpawns = 2f;

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(timeBetweenSpawns);
        }
    }

    void SpawnEnemy()
    {
        GameObject prefabToUse = enemyData.enemyPrefab;

        if (prefabToUse != null)
        {
            GameObject newEnemy = Instantiate(prefabToUse, spawnPoint.position, spawnPoint.rotation);

            EnemyMovement script = newEnemy.GetComponent<EnemyMovement>();
            if (script != null)
            {
                script.data = enemyData; 
            }
        }
        else
        {
            Debug.LogError("⚠️ La fiche " + enemyData.name + " n'a pas de Prefab associé !");
        }
    }
}