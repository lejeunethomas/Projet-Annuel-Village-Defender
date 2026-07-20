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

    private GameObject instance;

    public GameObject Instance
    {
        get { return instance; }
    }

    public void SetInstance(GameObject newInstance)
    {
        instance = newInstance;
    }
}

public class ResourceFarmManager : MonoBehaviour
{
    [Header("Fermes")]
    public List<ResourceFarm> farms = new List<ResourceFarm>();

    [Header("Production")]
    public float productionInterval = 1f;
    public int productionAmount = 1;

    private float productionTimer;

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

        productionTimer += Time.deltaTime;

        if (productionTimer < productionInterval)
            return;

        productionTimer = 0f;
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
}
