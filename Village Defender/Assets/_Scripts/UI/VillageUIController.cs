using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VillageUIController : MonoBehaviour
{
    [Header("Références")]
    public BuildingInventory inventory; 
    public TowerBuilder towerBuilder;

    [Header("Hotbar")]
    public GameObject hotbarPanel;
    public Transform hotbarContainer;
    public GameObject hotbarButtonPrefab;

    private void Start()
    {
        RefreshHotbar();
    }

    public void RefreshHotbar()
    {
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
}