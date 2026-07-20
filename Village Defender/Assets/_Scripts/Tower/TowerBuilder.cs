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
    public TMP_Text towerText;
    public TMP_Text modeButtonText;

    private bool _canBuild = false;
    private int _selectedCatalogIndex = -1;
    private BuildMode _currentMode = BuildMode.Build;

    private List<PlacedBuilding> _placedBuildings = new List<PlacedBuilding>();

    [System.Serializable]
    private class PlacedBuilding
    {
        public GameObject instance;
        public int catalogIndex;
    }

    private void Start()
    {
        UpdateModeButtonText();
        UpdateSelectedTowerText();
    }

    public void SetCanBuild(bool value)
    {
        _canBuild = value;

        if (!_canBuild)
            SetMode(BuildMode.Build);
    }

    public void SelectTowerFromCatalog(int index)
    {
        _selectedCatalogIndex = index;
        SetMode(BuildMode.Build);
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
        if (inventory == null || _selectedCatalogIndex < 0)
        {
            Debug.Log("Aucun bâtiment sélectionné.");
            return;
        }

        TowerData towerToBuild = inventory.GetBuilding(_selectedCatalogIndex);
        if (towerToBuild == null || towerToBuild.towerPrefab == null)
            return;

        if (Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 500f, buildLayerMask))
        {
            if (inventory.GetOwnedCount(_selectedCatalogIndex) > 0)
            {
                inventory.ConsumeBuilding(_selectedCatalogIndex);
            }
            else
            {
                bool achatReussi = inventory.BuyBuilding(_selectedCatalogIndex);
                if (!achatReussi)
                {
                    Debug.Log("Pas assez d'or pour construire cette tour !");
                    return;
                }
                inventory.ConsumeBuilding(_selectedCatalogIndex);
            }

            BuildTower(hit.point, towerToBuild);
        }
    }

    void TryRemoveTower()
    {
        if (Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit, 500f))
            return;

        int placedIndex;
        if (TryGetPlacedBuildingIndex(hit.transform, out placedIndex))
        {
            RemovePlacedBuilding(placedIndex);
        }
    }

    bool TryGetPlacedBuildingIndex(Transform hitTransform, out int placedIndex)
    {
        placedIndex = -1;
        if (hitTransform == null) return false;

        for (int i = 0; i < _placedBuildings.Count; i++)
        {
            PlacedBuilding placed = _placedBuildings[i];
            if (placed == null || placed.instance == null) continue;

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
        if (placedIndex < 0 || placedIndex >= _placedBuildings.Count) return;

        PlacedBuilding placed = _placedBuildings[placedIndex];
        if (placed == null) return;

        if (inventory != null)
            inventory.AddBuildingToStock(placed.catalogIndex, 1);

        if (placed.instance != null)
            Destroy(placed.instance);

        _placedBuildings.RemoveAt(placedIndex);
        UpdateSelectedTowerText();
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

        _placedBuildings.Add(new PlacedBuilding
        {
            instance = tower,
            catalogIndex = _selectedCatalogIndex
        });

        UpdateSelectedTowerText();
    }

    public void ReturnAllPlacedBuildingsToStock()
    {
        if (inventory == null) return;

        for (int i = _placedBuildings.Count - 1; i >= 0; i--)
        {
            PlacedBuilding placed = _placedBuildings[i];
            if (placed != null)
            {
                inventory.AddBuildingToStock(placed.catalogIndex, 1);
                if (placed.instance != null) Destroy(placed.instance);
            }
        }

        _placedBuildings.Clear();
        UpdateSelectedTowerText();
    }

    void UpdateSelectedTowerText()
    {
        if (towerText == null) return;

        if (_selectedCatalogIndex < 0 || inventory == null)
        {
            towerText.text = "Sélectionnez une tour dans la Hotbar";
            return;
        }

        TowerData data = inventory.GetBuilding(_selectedCatalogIndex);
        if (data != null)
        {
            int stock = inventory.GetOwnedCount(_selectedCatalogIndex);
            
            if (_currentMode == BuildMode.Delete)
                towerText.text = "Clic pour supprimer";
            else if (stock > 0)
                towerText.text = data.GetDisplayName() + " (En Stock : " + stock + ")";
            else
                towerText.text = data.GetDisplayName() + " (" + data.cost + " Or)";
        }
    }
}