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
    
    [Header("Panneau de Détails")]
    public GameObject detailsPanel;
    public TMP_Text detailNameText;
    public TMP_Text detailDescriptionText;
    public TMP_Text levelUpText;
    public Button upgradeButton;

    private int _currentSelectedTowerIndex = -1;
    private void Start()
    {
        RegisterInGameManager();
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
                    buttonComponent.onClick.AddListener(() => 
                    {
                        SelectTowerForBuilding(indexCatalogue);
                        OpenTowerDetails(indexCatalogue);       
                    });
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
            SetFarmButtonVisible(button, buttonText, false);
            return;
        }

        if (!resourceFarmManager.HasFarm(resourceType))
        {
            SetFarmButtonVisible(button, buttonText, false);
            return;
        }

        string displayName = resourceFarmManager.GetFarmDisplayName(resourceType);
        int cost = resourceFarmManager.GetFarmCost(resourceType);
        bool purchased = resourceFarmManager.IsFarmPurchased(resourceType);
        bool enoughGold = GameManager.Instance != null && GameManager.Instance.gold >= cost;

        if (purchased)
        {
            SetFarmButtonVisible(button, buttonText, false);
            return;
        }

        SetFarmButtonVisible(button, buttonText, true);

        if (buttonText != null)
            buttonText.text = displayName + " - " + cost + " or";

        if (button != null)
            button.interactable = enoughGold;
    }

    private void SetFarmButtonVisible(Button button, TMP_Text buttonText, bool visible)
    {
        if (button != null)
        {
            button.gameObject.SetActive(visible);
            button.interactable = visible;
        }

        if (buttonText != null)
            buttonText.gameObject.SetActive(visible);
    }

    private void RegisterInGameManager()
    {
        if (GameManager.Instance != null && GameManager.Instance.villageUIController == null)
            GameManager.Instance.villageUIController = this;
    }

    public void OpenTowerDetails(int index)
    {
        _currentSelectedTowerIndex = index;
        TowerData data = inventory.GetBuilding(index);
        int lv = inventory.GetTowerLevel(data.name);
        
        detailsPanel.SetActive(true);
        
        int degats = data.damage + (lv * data.bonusLv);
        int health = data.maxHealth + (lv * data.bonusLv);
        
        detailNameText.text = data.towerName;
        detailDescriptionText.text = "Dégâts : " + degats + 
                                     "\nRange : " + data.range + 
                                     "\nCadence : " + data.fireRate + 
                                     "\nVie : " + health + 
                                     "\nCible : " + data.targetType;
        levelUpText.text = "Level : "+ lv + "     " + data.lvCost + " or";
        if (upgradeButton != null)
        {
            upgradeButton.onClick.RemoveAllListeners();
            upgradeButton.onClick.AddListener(() => UpgradeSelectedTower());
        }
    }
    
    public void UpgradeSelectedTower()
    {
        if (_currentSelectedTowerIndex < 0) return;

        TowerData data = inventory.GetBuilding(_currentSelectedTowerIndex);
        int currentLevel = inventory.GetTowerLevel(data.name);
    
        int cost = data.lvCost + (currentLevel * 50); 

        if (GameManager.Instance.gold >= cost)
        {
            GameManager.Instance.SpendGold(cost);
        
            inventory.LevelUpTower(data.name);
        
            Debug.Log(data.name + " améliorée au niveau " + (currentLevel + 1) + " !");
        
            OpenTowerDetails(_currentSelectedTowerIndex); 
        }
        else
        {
            Debug.Log("Pas assez d'or pour améliorer !");
        }
    }
}
