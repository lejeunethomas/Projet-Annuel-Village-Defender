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
    public Button buildButton;
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
                    buttonComponent.onClick.AddListener(() => OpenTowerDetails(indexCatalogue));
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
        
        detailsPanel.SetActive(true);
        
        detailNameText.text = data.towerName;
        detailDescriptionText.text = "Dégâts : " + data.damage + 
                                     "\nRange : " + data.range + 
                                     "\nCadence : " + data.fireRate + 
                                     "\nVie : " + data.maxHealth + 
                                     "\nCible : " + data.targetType;
        
        buildButton.onClick.RemoveAllListeners();
        buildButton.onClick.AddListener(() => SelectTowerForBuilding(index));
        
        upgradeButton.onClick.RemoveAllListeners();
        upgradeButton.onClick.AddListener(() => UpgradeSelectedTower());
    }
    
    public void UpgradeSelectedTower()
    {
        Debug.Log("Amélioration de la tour " + _currentSelectedTowerIndex);
    }
}
