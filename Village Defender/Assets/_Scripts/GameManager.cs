using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("RESOURCES")]
    public int gold = 100;
    public int baseHealth = 10;
    
    [Header("État du jeu")]
    public int enemiesALive = 0;
    public bool isSpawningFinished = false;
    public bool runWave = false;
    
    [Header("UI")]
    public BaseHealthUI baseHealthUI;
    public GameObject gameOverScreen;


    void Awake()
    {
        Instance = this;
        
        if (gameOverScreen != null)
            gameOverScreen.SetActive(false);

    }
    
    public void RegisterEnemy(){ enemiesALive++; }

    public void UnregisterEnemmy()
    {
        enemiesALive--;
        CheckWinCondition();
    }

    public void CheckWinCondition()
    {
        if (isSpawningFinished && enemiesALive <= 0)
        {
            Victory();
        }
    }

    public void AddGold(int amount)
    {
        gold += amount;
        Debug.Log("Gold: " + gold);
    }

    public void DamageBase(int amount)
    {
        baseHealth -= amount;
        
        if (baseHealthUI != null)
            baseHealthUI.UpdateHealth(baseHealth);
        
        Debug.Log("Base HP: " + baseHealth);
        if (baseHealth <= 0)
        {
            Debug.Log("GAME OVER");
            Defeat();
        }
    }
    
    public void Victory()
    {
        
    }
    
    public void Defeat()
    {
        
    }
    
    
}