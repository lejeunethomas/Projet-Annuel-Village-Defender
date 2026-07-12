using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    public EnemyData data;
    private NavMeshAgent agent;
    private int currentHealth;
	private Transform Target;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        if (data != null)
        {
            agent.speed = data.moveSpeed;
            currentHealth = data.maxHealth;
        }

        if(TargetManager.Instance != null)
		{
			TargetManager.Instance.ennemisActifs.Add(this);
		}
    }
    
    void Update()
    {
		if(Target != null && agent.hasPath && !agent.pathPending)
		{
			if (agent.remainingDistance < 0.5f)
        	{
				if(Target.CompareTag("Base")){
					GameManager.Instance.DamageBase(1);
            		Die();
				}
        	}
		} 
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        if (currentHealth <= 0)
        {
            if (data != null) GameManager.Instance.AddGold(data.goldReward);
            Die();
        }
    }
    
    public void RecevoirNouvelleCible(Transform cible)
    {
        if (agent != null && cible != null)
        {
            agent.SetDestination(cible.position);
        }
    }

    void Die()
    {
        GameManager.Instance.UnregisterEnemy();
        Destroy(gameObject);
    }

    void OnDestroy() 
    {
        if(TargetManager.Instance != null)
            TargetManager.Instance.ennemisActifs.Remove(this);
    }
}