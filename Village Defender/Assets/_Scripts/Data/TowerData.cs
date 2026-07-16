using UnityEngine;

[CreateAssetMenu(fileName = "NewTower", menuName = "VillageDefender/TowerData")]
public class TowerData : ScriptableObject
{
    [Header("Infos")]
    public string towerName = "catapulte";
    public GameObject towerPrefab;

    [Header("Économie")]
    public int cost = 15;

    [Header("Stats")]
	public int maxHealth = 10;
    public int range = 5;
    public float fireRate = 1f;
    public int damage = 15;

	[Header("Cible")]
    public Type targetType = Type.None;
}
