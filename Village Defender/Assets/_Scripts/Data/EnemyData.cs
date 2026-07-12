using UnityEngine;

public enum TargetPriority
{
	BaseOnly = 0,
	TowersFirst = 1,
}

[CreateAssetMenu(fileName = "NewEnemy", menuName = "VillageDefender/EnemyData")]
public class EnemyData : ScriptableObject
{
    [Header("Infos de base")]
    public string enemyName = "Soldat";
    public GameObject enemyPrefab;

	[Header("Comportement IA")]
    [Tooltip("Définit ce que l'ennemi va chercher en priorité")]
    public TargetPriority priority = TargetPriority.BaseOnly;

    [Header("Stats")]
    public int maxHealth = 15;
    public float moveSpeed = 5f;
    public int goldReward = 5;
	
	[Header("Attaque")]
	public int attackDamage = 5;
	public float attackRate = 0.5f;
	public float attackRange = 1f;
	
}
