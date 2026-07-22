using System;
using System.Collections.Generic;
using UnityEngine;

public enum ResourceType
{
    Wood,
    Stone,
    Iron
}

[Serializable]
public class ResourceFarm
{
    public ResourceType resourceType;
    public string displayName;
    public int cost;
    public GameObject prefab;
    public Transform spawnPoint;
    public bool purchased;

    private GameObject _instance;

    public GameObject Instance
    {
        get { return _instance; }
    }

    public void SetInstance(GameObject newInstance)
    {
        _instance = newInstance;
    }
}

public class ResourceFarmManager : MonoBehaviour
{
    [Header("Fermes")]
    public List<ResourceFarm> farms = new List<ResourceFarm>();

    [Header("Production")]
    public float productionInterval = 1f;
    public int productionAmount = 1;

    private float _productionTimer;

    private void OnValidate()
    {
        if (productionInterval < 0.1f)
            productionInterval = 0.1f;

        if (productionAmount < 1)
            productionAmount = 1;
    }

    private void Update()
    {
        if (GameManager.Instance == null)
            return;

        _productionTimer += Time.deltaTime;

        if (_productionTimer < productionInterval)
            return;

        _productionTimer = 0f;
        ProduceResources();
    }

    public bool BuyFarm(ResourceType resourceType)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogError("Achat de ferme impossible : aucun GameManager.Instance trouve.");
            return false;
        }

        if (GameManager.Instance.CurrentPhase != GameManager.GamePhase.Village)
        {
            Debug.Log("Achat de ferme refuse : l'achat est autorise uniquement en phase Village.");
            return false;
        }

        ResourceFarm farm = GetFarm(resourceType);
        if (farm == null)
        {
            Debug.LogError("Achat de ferme impossible : aucune ferme configuree pour " + resourceType + ".");
            return false;
        }

        if (farm.purchased)
        {
            Debug.Log("Achat de ferme refuse : " + GetFarmDisplayName(resourceType) + " est deja achetee.");
            return false;
        }

        if (farm.prefab == null)
        {
            Debug.LogError("Achat de ferme impossible : aucun prefab assigne pour " + GetFarmDisplayName(resourceType) + ".");
            return false;
        }

        if (farm.spawnPoint == null)
        {
            Debug.LogError("Achat de ferme impossible : aucun point d'apparition assigne pour " + GetFarmDisplayName(resourceType) + ".");
            return false;
        }

        if (GameManager.Instance.gold < farm.cost)
        {
            Debug.Log("Achat de ferme refuse : pas assez d'or pour " + GetFarmDisplayName(resourceType) + ".");
            return false;
        }

        GameManager.Instance.SpendGold(farm.cost);

        GameObject farmInstance = Instantiate(farm.prefab, farm.spawnPoint.position, farm.spawnPoint.rotation);
        farm.SetInstance(farmInstance);
        farm.purchased = true;

        if (GameManager.Instance.villageUIController != null)
            GameManager.Instance.villageUIController.RefreshFarmUI();

        if (SaveManager.Instance != null)
            SaveManager.Instance.SaveGame();

        Debug.Log("Ferme achetee : " + GetFarmDisplayName(resourceType) + ".");
        return true;
    }

    public void BuyWoodFarm()
    {
        BuyFarm(ResourceType.Wood);
    }

    public void BuyStoneFarm()
    {
        BuyFarm(ResourceType.Stone);
    }

    public void BuyIronFarm()
    {
        BuyFarm(ResourceType.Iron);
    }

    public bool IsFarmPurchased(ResourceType resourceType)
    {
        ResourceFarm farm = GetFarm(resourceType);
        return farm != null && farm.purchased;
    }

    public float GetProductionPerSecond(ResourceType resourceType)
    {
        ResourceFarm farm = GetFarm(resourceType);
        if (farm == null || productionInterval <= 0f)
            return 0f;

        return productionAmount / productionInterval;
    }

    public void RestoreFarm(ResourceType resourceType, bool purchased)
    {
        ResourceFarm farm = GetFarm(resourceType);
        if (farm == null)
            return;

        farm.purchased = purchased;

        if (!purchased)
        {
            if (farm.Instance != null)
                Destroy(farm.Instance);

            farm.SetInstance(null);
            RefreshFarmUI();
            return;
        }

        if (farm.Instance != null)
        {
            RefreshFarmUI();
            return;
        }

        if (farm.prefab == null)
        {
            Debug.LogWarning("Restauration de ferme impossible : aucun prefab assigne pour " + GetFarmDisplayName(resourceType) + ".");
            RefreshFarmUI();
            return;
        }

        if (farm.spawnPoint == null)
        {
            Debug.LogWarning("Restauration de ferme impossible : aucun point d'apparition assigne pour " + GetFarmDisplayName(resourceType) + ".");
            RefreshFarmUI();
            return;
        }

        GameObject farmInstance = Instantiate(farm.prefab, farm.spawnPoint.position, farm.spawnPoint.rotation);
        farm.SetInstance(farmInstance);
        RefreshFarmUI();
    }

    public bool HasFarm(ResourceType resourceType)
    {
        return GetFarm(resourceType) != null;
    }

    public int GetFarmCost(ResourceType resourceType)
    {
        ResourceFarm farm = GetFarm(resourceType);
        return farm != null ? farm.cost : 0;
    }

    public string GetFarmDisplayName(ResourceType resourceType)
    {
        ResourceFarm farm = GetFarm(resourceType);
        if (farm == null)
            return resourceType.ToString();

        if (string.IsNullOrEmpty(farm.displayName))
            return resourceType.ToString();

        return farm.displayName;
    }

    private void ProduceResources()
    {
        if (farms == null)
            return;

        foreach (ResourceFarm farm in farms)
        {
            if (farm == null || !farm.purchased)
                continue;

            GameManager.Instance.AddResource(farm.resourceType, productionAmount);
        }
    }

    private ResourceFarm GetFarm(ResourceType resourceType)
    {
        if (farms == null)
            return null;

        foreach (ResourceFarm farm in farms)
        {
            if (farm != null && farm.resourceType == resourceType)
                return farm;
        }

        return null;
    }

    private void RefreshFarmUI()
    {
        if (GameManager.Instance != null && GameManager.Instance.villageUIController != null)
            GameManager.Instance.villageUIController.RefreshFarmUI();
    }
}
