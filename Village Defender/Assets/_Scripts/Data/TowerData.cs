using UnityEngine;

public enum GameEpoque
{
	Antiquité = 1,
	Rennaissance = 2,
	Comptenporaine = 3,
}

[CreateAssetMenu(fileName = "NewTower", menuName = "VillageDefender/TowerData")]
public class TowerData : ScriptableObject
{
    [Header("Infos")]
    public string towerName = "catapulte";
    public GameObject towerPrefab;
    public GameEpoque epoque = GameEpoque.Antiquité;

    [Header("Économie")]
    public int cost = 15;

    [Header("Stats")]
	public int maxHealth = 10;
    public int range = 5;
    public float fireRate = 1f;
    public int damage = 15;

	[Header("Cible")]
    public Type targetType = Type.None;

    public string GetDisplayName()
    {
        return string.IsNullOrWhiteSpace(towerName) ? name : towerName;
    }
}
