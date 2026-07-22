using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GamePhase
    {
        MainMenu,
        Intro,
        Village,
        Preparation,
        Wave,
        EndScreen
    }

    [Header("Ressources")]
    public int gold = 100;
    public int wood = 0;
    public int stone = 0;
    public int iron = 0;

    [Header("Base")]
    public int baseMaxHealth = 10;
    public int baseHealth = 10;

    [Header("Vagues")]
    public int currentWaveIndex = 0;
    public int enemiesAlive = 0;
    public bool isSpawningFinished = false;

    [Header("Époque")] 
    public int currentEpoch = 1;

    [Header("Références gameplay")]
    public SimpleSpawner spawner;
    public TowerBuilder towerBuilder;
    public BaseHealthUI baseHealthUI;
    
    [Header("UI Controllers")]
    public VillageUIController villageUIController;
    public BuildingInventory buildingInventory;

    [Header("UI")]
    public GameObject villageUI;
    public GameObject preparationUI;
    public GameObject waveUI;
    public GameObject hudUI;
    public GameOverUI endWaveUI;

    [Header("Récompenses")]
    public int endWaveBonusGold = 25;

    public GamePhase CurrentPhase { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        baseHealth = baseMaxHealth;

        if (baseHealthUI != null)
        {
            baseHealthUI.maxHealth = baseMaxHealth;
            baseHealthUI.UpdateHealth(baseHealth);
        }
        
        SetPhase(GamePhase.MainMenu);
    }

    public void SetPhase(GamePhase newPhase)
    {
        CurrentPhase = newPhase;

        if (villageUI != null)
            villageUI.SetActive(newPhase == GamePhase.Village);

        if (preparationUI != null)
            preparationUI.SetActive(newPhase == GamePhase.Preparation);

        if (waveUI != null)
            waveUI.SetActive(newPhase == GamePhase.Wave);

        if (endWaveUI != null)
            endWaveUI.gameObject.SetActive(newPhase == GamePhase.EndScreen);

        if (hudUI != null)
        {
            bool showHud = newPhase == GamePhase.Village ||
                           newPhase == GamePhase.Preparation ||
                           newPhase == GamePhase.Wave;
            hudUI.SetActive(showHud);
        }

        if (newPhase == GamePhase.Preparation)
        {
            if (villageUIController != null)
            {
                villageUIController.RefreshHotbar();
            }

            if (towerBuilder != null)
            {
                towerBuilder.SelectTowerFromCatalog(-1);
                towerBuilder.SetCanBuild(true);
            }
        }
        else 
        {
            if (villageUIController != null)
            {
                villageUIController.CloseAllPanels();
            }
            
            if (towerBuilder != null)
            {
                towerBuilder.SetCanBuild(false);
            }
        }

        Debug.Log("Phase actuelle : " + newPhase);
    }

    public void GoToPreparation()
    {
        Debug.Log("GoToPreparation appelé. Phase actuelle = " + CurrentPhase);

        if (CurrentPhase != GamePhase.Village)
        {
            Debug.Log("Refusé : on n'est pas en phase Village.");
            return;
        }

        SetPhase(GamePhase.Preparation);
    }

    public void LaunchWave()
    {
        if (CurrentPhase != GamePhase.Preparation)
            return;

        if (spawner == null)
        {
            Debug.LogError("Aucun SimpleSpawner assigné dans le GameManager.");
            return;
        }

        enemiesAlive = 0;
        isSpawningFinished = false;

        SetPhase(GamePhase.Wave);
        spawner.StartCurrentWave(currentWaveIndex);
    }
    
    public void ReturnToVillageFromPreparation()
    {
        if (CurrentPhase != GamePhase.Preparation)
            return;

        if (towerBuilder != null)
            towerBuilder.ReturnAllPlacedBuildingsToStock();

        SetPhase(GamePhase.Village);
    }
    public void ReturnToVillageAfterEndScreen(bool victory)
    {
        if (spawner != null)
            spawner.StopCurrentWaveAndDestroyEnemies();

        baseHealth = baseMaxHealth;

        if (baseHealthUI != null)
            baseHealthUI.UpdateHealth(baseHealth);

        enemiesAlive = 0;
        isSpawningFinished = false;

        if (towerBuilder != null)
            towerBuilder.ReturnAllPlacedBuildingsToStock();

        SetPhase(GamePhase.Village);

        if (SaveManager.Instance != null)
            SaveManager.Instance.SaveGame();
    }

    public void NextEpoque()
    {
        currentEpoch++;

        if (buildingInventory != null)
            buildingInventory.RemoveBuildingFromStock(currentEpoch);

        Debug.Log("🎉 Félicitations ! Passage à l'époque " + currentEpoch + " !");
        Victory();
    }

    public void RegisterEnemy()
    {
        enemiesAlive++;
        Debug.Log("Ennemis vivants : " + enemiesAlive);
    }

    public void UnregisterEnemy()
    {
        enemiesAlive = Mathf.Max(0, enemiesAlive - 1);
        Debug.Log("Ennemis vivants : " + enemiesAlive);
        CheckWinCondition();
    }

    public void NotifySpawningFinished()
    {
        isSpawningFinished = true;
        Debug.Log("Spawn terminé.");
        CheckWinCondition();
    }

    public void CheckWinCondition()
    {
        if (CurrentPhase == GamePhase.Wave && isSpawningFinished && enemiesAlive <= 0)
        {
            if (spawner != null &&
                spawner.waves != null &&
                currentWaveIndex >= 0 &&
                currentWaveIndex < spawner.waves.Count &&
                spawner.waves[currentWaveIndex] != null &&
                spawner.waves[currentWaveIndex].boss)
            {
                NextEpoque();
            }
            else
            {
                Victory();
            }
        }
    }

    public void AddGold(int amount)
    {
        gold += amount;
        Debug.Log("Gold : " + gold);
    }

    public void SpendGold(int amount)
    {
        gold -= amount;
        if (gold < 0)
            gold = 0;

        Debug.Log("Gold : " + gold);
    }

    public void AddResource(ResourceType type, int amount)
    {
        if (amount <= 0)
            return;
        
        switch (type)
        {
            case ResourceType.Wood:
                wood += amount;
                Debug.Log("Bois : " + wood);
                break;
            case ResourceType.Stone:
                stone += amount;
                Debug.Log("Pierre : " + stone);
                break;
            case ResourceType.Iron:
                iron += amount;
                Debug.Log("Fer : " + iron);
                break;
        }
    }

    public int GetResource(ResourceType type)
    {
        switch (type)
        {
            case ResourceType.Wood:
                return wood;
            case ResourceType.Stone:
                return stone;
            case ResourceType.Iron:
                return iron;
            default:
                return 0;
        }
    }

    public void AddWood(int amount)
    {
        AddResource(ResourceType.Wood, amount);
    }

    public bool SpendWood(int amount)
    {
        if (wood >= amount)
        {
            wood -= amount;
            return true;
        }
        Debug.Log("Pas assez de bois");
        return false;
    }

    public void AddStone(int amount)
    {
        AddResource(ResourceType.Stone, amount);
    }

    public bool SpendStone(int amount)
    {
        if (stone >= amount)
        {
            stone -= amount;
            return true;
        }
        Debug.Log("Pas assez de Pierre");
        return false;
    }

    public bool SpendIron(int amount)
    {
        if (iron >= amount)
        {
            iron -= amount;
            return true;
        }
        Debug.Log("Pas assez de Pierre");
        return false;
    }

    public void DamageBase(int amount)
    {
        if (CurrentPhase == GamePhase.EndScreen)
            return;

        baseHealth -= amount;

        if (baseHealth < 0)
            baseHealth = 0;

        if (baseHealthUI != null)
            baseHealthUI.UpdateHealth(baseHealth);

        Debug.Log("Base HP : " + baseHealth);

        if (baseHealth <= 0)
        {
            Defeat();
        }
    }

    public void Victory()
    {
        AddGold(endWaveBonusGold);
        currentWaveIndex++;

        SetPhase(GamePhase.EndScreen);

        if (endWaveUI != null)
            endWaveUI.ShowVictory();

        if (SaveManager.Instance != null)
            SaveManager.Instance.SaveGame();

        Debug.Log("Victoire : vague terminée.");
    }

    public void Defeat()
    {
        if (CurrentPhase == GamePhase.EndScreen)
            return;

        SetPhase(GamePhase.EndScreen);

        if (spawner != null)
            spawner.StopCurrentWaveAndDestroyEnemies();

        enemiesAlive = 0;
        isSpawningFinished = false;

        if (endWaveUI != null)
            endWaveUI.ShowDefeat();

        if (SaveManager.Instance != null)
            SaveManager.Instance.SaveGame();

        Debug.Log("Défaite : la base a été détruite.");
    }

    public bool IsBuildPhase()
    {
        return CurrentPhase == GamePhase.Preparation;
    }

    public void ApplyLoadedProgress(
        int loadedGold,
        int loadedWood,
        int loadedStone,
        int loadedIron,
        int loadedWaveIndex)
    {
        gold = Mathf.Max(0, loadedGold);
        wood = Mathf.Max(0, loadedWood);
        stone = Mathf.Max(0, loadedStone);
        iron = Mathf.Max(0, loadedIron);
        currentWaveIndex = Mathf.Max(0, loadedWaveIndex);

        baseHealth = baseMaxHealth;

        if (baseHealthUI != null)
            baseHealthUI.UpdateHealth(baseHealth);

        enemiesAlive = 0;
        isSpawningFinished = false;
    }

    public void PrepareForLoadedGame()
    {
        if (spawner != null)
            spawner.StopCurrentWaveAndDestroyEnemies();

        DestroyRemainingEnemiesInScene();

        baseHealth = baseMaxHealth;

        if (baseHealthUI != null)
            baseHealthUI.UpdateHealth(baseHealth);

        enemiesAlive = 0;
        isSpawningFinished = false;

        if (towerBuilder != null)
            towerBuilder.ClearPlacedBuildingsForLoadedGame();

        if (villageUIController != null)
            villageUIController.CloseAllPanels();
    }

    public void RefreshLoadedGameUI()
    {
        if (baseHealthUI != null)
            baseHealthUI.UpdateHealth(baseHealth);

        if (hudUI != null)
        {
            HUDUI hud = hudUI.GetComponent<HUDUI>();
            if (hud != null)
                hud.Refresh();
        }

        if (villageUIController != null)
            villageUIController.RefreshFarmUI();
    }

    private void DestroyRemainingEnemiesInScene()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        for (int i = 0; i < enemies.Length; i++)
        {
            if (enemies[i] != null)
                Destroy(enemies[i]);
        }
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
