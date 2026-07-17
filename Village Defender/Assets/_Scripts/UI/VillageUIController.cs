using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VillageUIController : MonoBehaviour
{
    [Header("Références")]
    public BuildingInventory inventory;
    public TowerBuilder towerBuilder;

    [Header("Panels")]
    public GameObject shopPanel;
    public GameObject stockPanel;

    [Header("Boutique (Génération Auto)")]
    public Transform shopContainer;
    public GameObject shopItemPrefab;

    [Header("Stock")]
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
        if (inventory == null || GameManager.Instance == null) return;
        
        int epoqueActuelle = GameManager.Instance.currentEpoch;

        if (shopContainer != null && shopItemPrefab != null)
        {
            foreach (Transform child in shopContainer)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < inventory.GetCatalogCount(); i++)
            {
                TowerData data = inventory.GetBuilding(i);

                if (data != null && (int)data.epoque == epoqueActuelle)
                {
                    GameObject newBtn = Instantiate(shopItemPrefab, shopContainer);
                    
                    TMP_Text btnText = newBtn.GetComponentInChildren<TMP_Text>();
                    if (btnText != null)
                    {
                        btnText.text = data.GetDisplayName() + " (" + data.cost + " or)";
                    }

                    Button buttonComponent = newBtn.GetComponent<Button>();
                    if (buttonComponent != null)
                    {
                        int indexCatalogue = i;
                        buttonComponent.onClick.AddListener(() => BuyBuilding(indexCatalogue));
                    }
                }
            }
        }

        if (stockText != null)
        {
            string content = "Stock :\n\n";

            for (int i = 0; i < inventory.GetCatalogCount(); i++)
            {
                TowerData data = inventory.GetBuilding(i);
                int count = inventory.GetOwnedCount(i);

                if (data != null && ((int)data.epoque == epoqueActuelle || count > 0))
                {
                    content += "- " + data.GetDisplayName() + " x" + count + "\n";
                }
            }
            stockText.text = content;
        }
    }
}
