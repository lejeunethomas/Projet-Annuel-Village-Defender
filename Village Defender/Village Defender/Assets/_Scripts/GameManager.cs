using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance; // Singleton pour accès facile

    public int gold = 100;
    public int baseHealth = 10;
    
    void Awake() { Instance = this; }

    public void AddGold(int amount)
    {
        gold += amount;
        Debug.Log("Gold: " + gold);
    }

    public void DamageBase(int amount)
    {
        baseHealth -= amount;
        Debug.Log("Base HP: " + baseHealth);
        if (baseHealth <= 0)
        {
            Debug.Log("GAME OVER");
            // Recharge la scène pour recommencer (simple pour le prototype)
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}