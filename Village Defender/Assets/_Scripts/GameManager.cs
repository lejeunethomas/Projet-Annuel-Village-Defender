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
    
    public BaseHealthUI baseHealthUI;
    
    void Awake() { Instance = this; }
    
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

    public void Victory()
    {
        
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
            // Recharge la scène pour recommencer (simple pour le prototype)
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}