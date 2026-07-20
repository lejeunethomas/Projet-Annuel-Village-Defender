using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TowerBuilder : MonoBehaviour
{
    public enum BuildMode
    {
        Build,
        Delete
    }

    [Header("Références")]
    public BuildingInventory inventory;

    [Header("Placement")]
    public LayerMask buildLayerMask = ~0;
    public float buildHeight = 0.5f;

    [Header("UI préparation")]
    public TMP_Dropdown selectBatDropdown;
    public TMP_Text towerText;
    public TMP_Text modeButtonText;

    private bool _canBuild = false;
    private int _selectedCatalogIndex = -1;
    private BuildMode _currentMode = BuildMode.Build;

    private List<int> _dropdownToCatalogIndex = new List<int>();
    private List<PlacedBuilding> _placedBuildings = new List<PlacedBuilding>();

    [System.Serializable]
    private class PlacedBuilding
    {
        public GameObject instance;
        public int catalogIndex;
    }

    public void SetCanBuild(bool value)
    {
        _canBuild = value;

        if (!_canBuild)
            SetMode(BuildMode.Build);
    }

    private void Start()
    {
        RefreshStockDropdown();
        UpdateModeButtonText();
    }

    public void RefreshStockDropdown()
    {
        _dropdownToCatalogIndex.Clear();

        if (selectBatDropdown != null)
        {
            selectBatDropdown.onValueChanged.RemoveAllListeners();
            selectBatDropdown.ClearOptions();
        }

        if (inventory == null)
        {
            UpdateSelectedTowerText();
            return;
        }

        List<string> options = new List<string>();

        for (int i = 0; i < inventory.GetCatalogCount(); i++)
        {
            TowerData data = inventory.GetBuilding(i);
            int count = inventory.GetOwnedCount(i);

            if (data != null && count > 0)
            {
                _dropdownToCatalogIndex.Add(i);
                options.Add(data.GetDisplayName() + " x" + count);
            }
        }

        if (selectBatDropdown != null)
        {
            if (options.Count > 0)
            {
                selectBatDropdown.AddOptions(options);
                selectBatDropdown.value = 0;
                selectBatDropdown.RefreshShownValue();
                selectBatDropdown.onValueChanged.AddListener(OnDropdownChanged);

                OnDropdownChanged(0);
            }
            else
            {
                _selectedCatalogIndex = -1;
            }
        }
        else
        {
            _selectedCatalogIndex = _dropdownToCatalogIndex.Count > 0 ? _dropdownToCatalogIndex[0] : -1;
        }

        UpdateSelectedTowerText();
    }

    public void ToggleBuildDeleteMode()
    {
        if (!_canBuild)
            return;

        if (_currentMode == BuildMode.Build)
            SetMode(BuildMode.Delete);
        else
            SetMode(BuildMode.Build);
    }

    private void SetMode(BuildMode newMode)
    {
        _currentMode = newMode;
        UpdateModeButtonText();
        UpdateSelectedTowerText();
    }

    private void UpdateModeButtonText()
    {
        if (modeButtonText == null)
            return;

        modeButtonText.text = _currentMode == BuildMode.Build
            ? "Mode : Construction"
            : "Mode : Suppression";
    }

    void OnDropdownChanged(int dropdownIndex)
    {
        if (dropdownIndex < 0 || dropdownIndex >= _dropdownToCatalogIndex.Count)
        {
            _selectedCatalogIndex = -1;
        }
        else
        {
            _selectedCatalogIndex = _dropdownToCatalogIndex[dropdownIndex];
        }

        UpdateSelectedTowerText();
    }

    void Update()
    {
        if (!_canBuild)
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButtonDown(0))
        {
            if (_currentMode == BuildMode.Build)
                TryBuild();
            else
                TryRemoveTower();
        }
    }

    void TryBuild()
    {
        if (inventory == null)
        {
            Debug.LogError("Aucun BuildingInventory assigné dans TowerBuilder.");
            return;
        }

        if (_selectedCatalogIndex < 0)
        {
            Debug.Log("Aucun bâtiment disponible dans le stock.");
            return;
        }

        TowerData towerToBuild = inventory.GetBuilding(_selectedCatalogIndex);
        if (towerToBuild == null)
        {
            Debug.LogError("Le bâtiment sélectionné est null.");
            return;
        }

        if (towerToBuild.towerPrefab == null)
        {
            Debug.LogError("Le prefab du bâtiment sélectionné est null.");
            return;
        }

        if (inventory.GetOwnedCount(_selectedCatalogIndex) <= 0)
        {
            Debug.Log("Tu ne possèdes plus ce bâtiment.");
            RefreshStockDropdown();
            return;
        }

        if (Camera.main == null)
        {
            Debug.LogError("Aucune caméra avec le tag MainCamera.");
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 500f, buildLayerMask))
        {
            BuildTower(hit.point, towerToBuild);
        }
        else
        {
            Debug.Log("Le rayon n'a rien touché. Vérifie le collider du sol et le LayerMask.");
        }
    }

    void TryRemoveTower()
    {
        if (Camera.main == null)
        {
            Debug.LogError("Aucune caméra avec le tag MainCamera.");
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit, 500f))
            return;

        int placedIndex;
        if (TryGetPlacedBuildingIndex(hit.transform, out placedIndex))
        {
            RemovePlacedBuilding(placedIndex);
        }
        else
        {
            Debug.Log("Aucune tour placée sélectionnée.");
        }
    }

    bool TryGetPlacedBuildingIndex(Transform hitTransform, out int placedIndex)
    {
        placedIndex = -1;

        if (hitTransform == null)
            return false;

        for (int i = 0; i < _placedBuildings.Count; i++)
        {
            PlacedBuilding placed = _placedBuildings[i];

            if (placed == null || placed.instance == null)
                continue;

            Transform current = hitTransform;

            while (current != null)
            {
                if (current.gameObject == placed.instance)
                {
                    placedIndex = i;
                    return true;
                }

                current = current.parent;
            }
        }

        return false;
    }

    void RemovePlacedBuilding(int placedIndex)
    {
        if (placedIndex < 0 || placedIndex >= _placedBuildings.Count)
            return;

        PlacedBuilding placed = _placedBuildings[placedIndex];
        if (placed == null)
            return;

        if (inventory != null)
            inventory.AddBuildingToStock(placed.catalogIndex, 1);

        if (placed.instance != null)
            Destroy(placed.instance);

        _placedBuildings.RemoveAt(placedIndex);
        RefreshStockDropdown();
    }

    void BuildTower(Vector3 position, TowerData towerToBuild)
    {
        Vector3 spawnPosition = position;
        spawnPosition.y = buildHeight;

        GameObject tower = Instantiate(towerToBuild.towerPrefab, spawnPosition, Quaternion.identity);

        TowerCombat combat = tower.GetComponent<TowerCombat>();
        if (combat != null)
        {
            combat.data = towerToBuild;
        }

        inventory.ConsumeBuilding(_selectedCatalogIndex);

        _placedBuildings.Add(new PlacedBuilding
        {
            instance = tower,
            catalogIndex = _selectedCatalogIndex
        });

        RefreshStockDropdown();
    }

    public void ReturnAllPlacedBuildingsToStock()
    {
        if (inventory == null)
            return;

        for (int i = _placedBuildings.Count - 1; i >= 0; i--)
        {
            PlacedBuilding placed = _placedBuildings[i];

            if (placed != null)
            {
                TowerData data = inventory.GetBuilding(placed.catalogIndex);
                if (data != null)
                {
                    inventory.AddBuildingToStock(placed.catalogIndex, 1);
                }

                if (placed.instance != null)
                {
                    Destroy(placed.instance);
                }
            }
        }

        _placedBuildings.Clear();
        RefreshStockDropdown();
    }

    void UpdateSelectedTowerText()
    {
        if (towerText == null)
            return;

        if (inventory == null)
        {
            towerText.text = "Aucun bâtiment disponible";
            return;
        }
        
        TowerData data = inventory.GetBuilding(_selectedCatalogIndex);
        int count = inventory.GetOwnedCount(_selectedCatalogIndex);

        if (_currentMode == BuildMode.Delete)
        {
            towerText.text = data != null
                ? data.GetDisplayName() + " | Stock : " + count
                : "Aucun bâtiment disponible";
            return;
        }

        if (inventory == null || _selectedCatalogIndex < 0)
        {
            towerText.text = "Aucun bâtiment disponible";
            return;
        }

        if (data == null)
        {
            towerText.text = "Aucun bâtiment disponible";
            return;
        }

        towerText.text = data.GetDisplayName() + " | Stock : " + count;
    }
}
