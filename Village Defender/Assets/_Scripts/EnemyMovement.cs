using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    public EnemyData data;
    private NavMeshAgent agent;
    private int currentHealth;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        if (data != null)
        {
            agent.speed = data.moveSpeed;
            currentHealth = data.maxHealth;
        }

        GameObject baseObj = GameObject.FindWithTag("Base");
        if (baseObj != null)
            agent.SetDestination(baseObj.transform.position);
    }
    
    void Update()
    {
        // Si l'ennemi est arrivé à la base
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            GameManager.Instance.DamageBase(1);
            GameManager.Instance.UnregisterEnemmy()
            Destroy(gameObject); // L'ennemi disparaît
        }
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (data != null)
            GameManager.Instance.AddGold(data.goldReward);
            
        GameManager.Instance.UnregisterEnemmy();
        Destroy(gameObject);
    }
}