using System.Collections.Generic;
using UnityEngine;

public class BuildingInventory : MonoBehaviour
{
    public static BuildingInventory Instance;

    [Header("Liste bâtiments achetables")]
    public List<TowerData> buildingCatalog = new List<TowerData>();

    [Header("Stock possédé")]
    [SerializeField] private List<int> ownedCounts = new List<int>();
    
    [Header("Améliorations")]
    private Dictionary<string, int> _towerLevels = new Dictionary<string, int>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        EnsureListSize();
        InitTowerLevel();
    }

    private void OnValidate()
    {
        EnsureListSize();
    }

    void EnsureListSize()
    {
        if (buildingCatalog == null)
            buildingCatalog = new List<TowerData>();

        if (ownedCounts == null)
            ownedCounts = new List<int>();

        while (ownedCounts.Count < buildingCatalog.Count)
            ownedCounts.Add(0);

        while (ownedCounts.Count > buildingCatalog.Count)
            ownedCounts.RemoveAt(ownedCounts.Count - 1);
    }

    public int GetCatalogCount()
    {
        return buildingCatalog.Count;
    }

    public TowerData GetBuilding(int catalogIndex)
    {
        if (catalogIndex < 0 || catalogIndex >= buildingCatalog.Count)
            return null;

        return buildingCatalog[catalogIndex];
    }

    public int GetOwnedCount(int catalogIndex)
    {
        if (catalogIndex < 0 || catalogIndex >= ownedCounts.Count)
            return 0;

        return ownedCounts[catalogIndex];
    }

    public bool BuyBuilding(int catalogIndex)
    {
        TowerData data = GetBuilding(catalogIndex);
        if (data == null)
        {
            Debug.LogError("Bâtiment invalide dans l'inventaire.");
            return false;
        }

        if (GameManager.Instance == null)
            return false;

        if (GameManager.Instance.currentEpoch < (int)data.epoque)
        {
            Debug.Log("Impossible d'acheter : " + data.GetDisplayName() + ". Nécessite l'époque " + data.epoque);
            return false;
        }

        if (GameManager.Instance.gold < data.cost)
        {
            Debug.Log("Pas assez d'or pour acheter : " + data.GetDisplayName());
            return false;
        }

        GameManager.Instance.SpendGold(data.cost);
        ownedCounts[catalogIndex]++;
        Debug.Log("Achat : " + data.GetDisplayName() + " | Stock = " + ownedCounts[catalogIndex]);
        return true;
    }

    public bool ConsumeBuilding(int catalogIndex)
    {
        if (catalogIndex < 0 || catalogIndex >= ownedCounts.Count)
            return false;

        if (ownedCounts[catalogIndex] <= 0)
        {
            Debug.Log("Stock vide pour ce bâtiment.");
            return false;
        }

        ownedCounts[catalogIndex]--;
        return true;
    }
    public void AddBuildingToStock(int catalogIndex, int amount = 1)
    {
        if (catalogIndex < 0 || catalogIndex >= ownedCounts.Count)
            return;

        ownedCounts[catalogIndex] += amount;
    }

    public void RemoveBuildingFromStock(int NewEpoque)
    {
        for (int i = 0; i < ownedCounts.Count; i++)
        {
            TowerData data = buildingCatalog[i];
            if (data != null && (int)data.epoque < NewEpoque && ownedCounts[i] > 0)
            {
                int Tower = ownedCounts[i];
                int Return = Tower * data.cost;
                
                GameManager.Instance.AddGold(Return);
                
                ownedCounts[i] = 0;
                
                Debug.Log($"Liquidation : {Tower}x {data.GetDisplayName()} repris. Remboursement : {Return} or.");
            }
        }
    }

    public void InitTowerLevel()
    {
        _towerLevels.Clear();
        for (int i = 0; i < GetCatalogCount(); i++)
        {
            TowerData data = GetBuilding(i);
            if (data != null)
            {
                _towerLevels.Add(data.name, 0);
            }
        }
    }

    public int GetTowerLevel(string towerName)
    {
        if (_towerLevels.ContainsKey(towerName))
        {
            return _towerLevels[towerName];
        }
        return 0;
    }

    public void LevelUpTower(string towerName)
    {
        if (_towerLevels.ContainsKey(towerName))
        {
            _towerLevels[towerName]++;
        }
    }
}
