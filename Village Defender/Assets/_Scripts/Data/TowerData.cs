using UnityEngine;
using UnityEngine.Serialization;

public enum GameEpoque
{
	AgeDePierre = 1,
	Antiquité = 2,
	Rennaissance = 3,
	Comptenporaine = 4,
}

[CreateAssetMenu(fileName = "NewTower", menuName = "VillageDefender/TowerData")]
public class TowerData : ScriptableObject
{
    [Header("Infos")]
    public string towerName = "catapulte";
    public GameObject towerPrefab;
    public GameEpoque epoque = GameEpoque.Antiquité;
    public Sprite towerIcon;
	public Animator characterAnimator;

    [Header("Économie")]
    public int cost = 15;
    public int lvCost = 15;

    [Header("Déblocage")]
    [Min(0)]
    public int unlockWaveIndex = 0;

    [Header("Stats")]
	public int maxHealth = 10;
    public int range = 5;
    public float fireRate = 1f;
    public int damage = 15;
    public int bonusLv = 3;

	[Header("Cible")]
    public Type targetType = Type.None;

    public string GetDisplayName()
    {
        return string.IsNullOrWhiteSpace(towerName) ? name : towerName;
    }

    public bool IsUnlocked(int currentWaveIndex)
    {
        return currentWaveIndex >= unlockWaveIndex;
    }
}
