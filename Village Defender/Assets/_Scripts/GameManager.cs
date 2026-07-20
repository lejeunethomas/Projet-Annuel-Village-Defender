using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public enum GamePhase
    {
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
    public int Wood = 0;
    public int Stone = 0;

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
        
        SetPhase(GamePhase.Village);
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

        if (towerBuilder != null)
            towerBuilder.SetCanBuild(newPhase == GamePhase.Preparation);

        if (newPhase == GamePhase.Preparation && towerBuilder != null)
            towerBuilder.RefreshStockDropdown();

        if (newPhase != GamePhase.Village && villageUIController != null)
            villageUIController.CloseAllPanels();

        if (villageUIController != null)
            villageUIController.RefreshTexts();

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
        baseHealth = baseMaxHealth;

        if (baseHealthUI != null)
            baseHealthUI.UpdateHealth(baseHealth);

        enemiesAlive = 0;
        isSpawningFinished = false;

        if (towerBuilder != null)
            towerBuilder.ReturnAllPlacedBuildingsToStock();

        SetPhase(GamePhase.Village);
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
                spawner.waves[currentWaveIndex].Boss)
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
                Wood = wood;
                Debug.Log("Bois : " + wood);
                break;
            case ResourceType.Stone:
                stone += amount;
                Stone = stone;
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
            Wood = wood;
            return true;
        }
        else
        {
            Debug.Log("Pas assez de bois");
            return false;
        }
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
            Stone = stone;
            return true;
        }
        else
        {
            Debug.Log("Pas assez de Pierre");
            return false;
        }
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

        Debug.Log("Victoire : vague terminée.");
    }

    public void Defeat()
    {
        SetPhase(GamePhase.EndScreen);

        if (endWaveUI != null)
            endWaveUI.ShowDefeat();

        Debug.Log("Défaite : la base a été détruite.");
    }

    public bool IsBuildPhase()
    {
        return CurrentPhase == GamePhase.Preparation;
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
