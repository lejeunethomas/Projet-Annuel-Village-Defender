using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VillageUIController : MonoBehaviour
{
    [Header("Références")]
    public BuildingInventory inventory; 
    public TowerBuilder towerBuilder;
    public ResourceFarmManager resourceFarmManager;

    [Header("Hotbar")]
    public GameObject hotbarPanel;
    public Transform hotbarContainer;
    public GameObject hotbarButtonPrefab;

    [Header("Fermes")]
    public Button woodFarmButton;
    public Button stoneFarmButton;
    public Button ironFarmButton;
    public TMP_Text woodFarmButtonText;
    public TMP_Text stoneFarmButtonText;
    public TMP_Text ironFarmButtonText;

    private void Start()
    {
        CloseAllPanels();
        RefreshHotbar();
        RefreshFarmUI();
    }

    public void RefreshHotbar()
    {
        if (hotbarPanel != null) hotbarPanel.SetActive(true); 

        RefreshFarmUI();

        if (inventory == null || GameManager.Instance == null) return;
        
        int epoqueActuelle = GameManager.Instance.currentEpoch;

        foreach (Transform child in hotbarContainer)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < inventory.GetCatalogCount(); i++)
        {
            TowerData data = inventory.GetBuilding(i);

            if (data != null && (int)data.epoque == epoqueActuelle)
            {
                GameObject newBtn = Instantiate(hotbarButtonPrefab, hotbarContainer);
                
                TMP_Text btnText = newBtn.GetComponentInChildren<TMP_Text>();
                if (btnText != null)
                {
                    btnText.text = data.cost + " Or";
                }
                
                Transform iconTransform = newBtn.transform.Find("Icon");
                if (iconTransform != null)
                {
                    Image iconImage = iconTransform.GetComponent<Image>();
                    if (iconImage != null && data.towerIcon != null)
                    {
                        iconImage.sprite = data.towerIcon;
                    }
                }

                Button buttonComponent = newBtn.GetComponent<Button>();
                if (buttonComponent != null)
                {
                    int indexCatalogue = i;
                    buttonComponent.onClick.AddListener(() => SelectTowerForBuilding(indexCatalogue));
                }
            }
        }
    }

    public void CloseAllPanels()
    {
        if (hotbarPanel != null)
        {
            hotbarPanel.SetActive(false);
        }
    }
    
    public void SelectTowerForBuilding(int index)
    {
        towerBuilder.SelectTowerFromCatalog(index);

        Debug.Log("Tour sélectionnée : " + inventory.GetBuilding(index).name);
    }

    public void RefreshFarmUI()
    {
        RefreshOneFarmUI(ResourceType.Wood, woodFarmButton, woodFarmButtonText);
        RefreshOneFarmUI(ResourceType.Stone, stoneFarmButton, stoneFarmButtonText);
        RefreshOneFarmUI(ResourceType.Iron, ironFarmButton, ironFarmButtonText);
    }

    private void RefreshOneFarmUI(ResourceType resourceType, Button button, TMP_Text buttonText)
    {
        if (button == null && buttonText == null)
            return;

        if (resourceFarmManager == null)
        {
            if (button != null)
                button.interactable = false;

            if (buttonText != null)
                buttonText.text = "Ferme indisponible";

            return;
        }

        if (!resourceFarmManager.HasFarm(resourceType))
        {
            if (button != null)
                button.interactable = false;

            if (buttonText != null)
                buttonText.text = "Ferme indisponible";

            return;
        }

        string displayName = resourceFarmManager.GetFarmDisplayName(resourceType);
        int cost = resourceFarmManager.GetFarmCost(resourceType);
        bool purchased = resourceFarmManager.IsFarmPurchased(resourceType);
        bool enoughGold = GameManager.Instance != null && GameManager.Instance.gold >= cost;

        if (buttonText != null)
        {
            if (purchased)
                buttonText.text = displayName + " - Achet\u00e9e";
            else
                buttonText.text = displayName + " - " + cost + " or";
        }

        if (button != null)
            button.interactable = !purchased && enoughGold;
    }
}
