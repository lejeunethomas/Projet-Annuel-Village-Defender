using UnityEngine;
using System.Collections;

public class SimpleSpawner : MonoBehaviour
{
    public GameObject enemyPrefab;
    public Transform spawnPoint;
    public Transform baseTarget; // La cible que les ennemis doivent atteindre
    public float timeBetweenWaves = 5f;

    void Start()
    {
        StartCoroutine(SpawnWave());
    }

    IEnumerator SpawnWave()
    {
        while (true) // Boucle infinie pour le proto
        {
            SpawnEnemy();
            yield return new WaitForSeconds(2f); // Un ennemi toutes les 2 secondes
        }
    }

    void SpawnEnemy()
    {
        GameObject newEnemy = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        // On assigne la cible à l'ennemi
        newEnemy.GetComponent<EnemyMovement>().targetBase = baseTarget;
    }
}