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
            
        Destroy(gameObject);
    }
}