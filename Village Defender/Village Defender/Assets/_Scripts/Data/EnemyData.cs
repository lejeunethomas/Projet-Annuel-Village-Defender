using UnityEngine;

[CreateAssetMenu(fileName = "NewEnemy", menuName = "VillageDefender/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("Infos de base")]
    public string enemyName = "Gobelin";
    public GameObject enemyPrefab;

    [Header("Stats")]
    public int maxHealth = 15;
    public float moveSpeed = 5f;
    public int goldReward = 5;
}
