using TMPro;
using UnityEngine;

public class VillageUIController : MonoBehaviour
{
    [Header("Références")]
    public BuildingInventory inventory;
    public TowerBuilder towerBuilder;

    [Header("Panels")]
    public GameObject shopPanel;
    public GameObject stockPanel;

    [Header("Textes")]
    public TMP_Text shopText;
    public TMP_Text stockText;

    private void Start()
    {
        CloseAllPanels();
        RefreshTexts();
    }

    public void ToggleShopPanel()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentPhase != GameManager.GamePhase.Village)
            return;

        bool newState = !shopPanel.activeSelf;
        shopPanel.SetActive(newState);

        if (stockPanel != null && newState)
            stockPanel.SetActive(false);

        RefreshTexts();
    }

    public void ToggleStockPanel()
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentPhase != GameManager.GamePhase.Village)
            return;

        bool newState = !stockPanel.activeSelf;
        stockPanel.SetActive(newState);

        if (shopPanel != null && newState)
            shopPanel.SetActive(false);

        RefreshTexts();
    }

    public void CloseAllPanels()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);

        if (stockPanel != null)
            stockPanel.SetActive(false);
    }

    public void BuyBuilding(int catalogIndex)
    {
        if (GameManager.Instance == null || GameManager.Instance.CurrentPhase != GameManager.GamePhase.Village)
            return;

        if (inventory == null)
            return;

        bool success = inventory.BuyBuilding(catalogIndex);

        if (success && towerBuilder != null)
            towerBuilder.RefreshStockDropdown();

        RefreshTexts();
    }

    public void RefreshTexts()
    {
        if (inventory == null)
            return;

        if (shopText != null)
        {
            string content = "Boutique :\n";

            for (int i = 0; i < inventory.GetCatalogCount(); i++)
            {
                TowerData data = inventory.GetBuilding(i);
                if (data != null)
                {
                    content += "- " + data.name + " : " + data.cost + " or\n";
                }
            }

            shopText.text = content;
        }

        if (stockText != null)
        {
            string content = "Stock :\n";

            for (int i = 0; i < inventory.GetCatalogCount(); i++)
            {
                TowerData data = inventory.GetBuilding(i);
                if (data != null)
                {
                    content += "- " + data.name + " x" + inventory.GetOwnedCount(i) + "\n";
                }
            }

            stockText.text = content;
        }
    }
}