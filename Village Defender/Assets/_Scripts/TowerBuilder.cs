using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class TowerBuilder : MonoBehaviour
{
    [Header("Bâtiments disponibles")]
    public List<TowerData> availableTowers = new List<TowerData>();
    public TowerData towerToBuild;

    [Header("Placement")]
    public LayerMask buildLayerMask = ~0;
    public float buildHeight = 0.5f;

    [Header("UI Préparation")]
    public TMP_Dropdown selectBatDropdown;
    public TMP_Text towerText;

    private bool canBuild = false;

    void Start()
    {
        SetupDropdown();
    }

    public void SetCanBuild(bool value)
    {
        canBuild = value;
    }

    public void SetupDropdown()
    {
        if (selectBatDropdown == null)
        {
            UpdateSelectedTowerText();
            return;
        }

        selectBatDropdown.onValueChanged.RemoveAllListeners();
        selectBatDropdown.ClearOptions();

        List<string> options = new List<string>();

        foreach (TowerData tower in availableTowers)
        {
            if (tower != null)
                options.Add(tower.name);
        }

        if (options.Count == 0)
        {
            towerToBuild = null;
            UpdateSelectedTowerText();
            return;
        }

        selectBatDropdown.AddOptions(options);
        selectBatDropdown.value = 0;
        selectBatDropdown.RefreshShownValue();
        selectBatDropdown.onValueChanged.AddListener(SelectTowerByIndex);

        SelectTowerByIndex(0);
    }

    public void SelectTowerByIndex(int index)
    {
        if (availableTowers == null || availableTowers.Count == 0)
        {
            towerToBuild = null;
            UpdateSelectedTowerText();
            return;
        }

        if (index < 0 || index >= availableTowers.Count)
            index = 0;

        towerToBuild = availableTowers[index];
        UpdateSelectedTowerText();
    }

    void Update()
    {
        if (!canBuild)
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (Input.GetMouseButtonDown(0))
        {
            TryBuild();
        }
    }

    void TryBuild()
    {
        if (towerToBuild == null)
        {
            Debug.LogError("Aucun bâtiment sélectionné.");
            return;
        }

        if (towerToBuild.towerPrefab == null)
        {
            Debug.LogError("Le prefab du bâtiment sélectionné est null.");
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
            if (GameManager.Instance.gold < towerToBuild.cost)
            {
                Debug.Log("Pas assez d'or. Il faut " + towerToBuild.cost);
                return;
            }

            BuildTower(hit.point);
        }
        else
        {
            Debug.Log("Le rayon n'a rien touché. Vérifie le collider du sol et le LayerMask.");
        }
    }

    void BuildTower(Vector3 position)
    {
        Vector3 spawnPosition = position;
        spawnPosition.y = buildHeight;

        GameObject tower = Instantiate(towerToBuild.towerPrefab, spawnPosition, Quaternion.identity);

        TowerCombat combat = tower.GetComponent<TowerCombat>();
        if (combat != null)
        {
            combat.data = towerToBuild;
        }

        GameManager.Instance.SpendGold(towerToBuild.cost);
        UpdateSelectedTowerText();
    }

    void UpdateSelectedTowerText()
    {
        if (towerText == null)
            return;

        if (towerToBuild == null)
        {
            towerText.text = "Aucun bâtiment sélectionné";
        }
        else
        {
            towerText.text = towerToBuild.name + " - Coût : " + towerToBuild.cost + " or";
        }
    }
}