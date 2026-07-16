using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    public EnemyData data;
    private NavMeshAgent agent;
    private int currentHealth;
	private Transform Target;
	private float attackTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        if (data != null)
        {
            agent.speed = data.moveSpeed;
			agent.stoppingDistance = data.attackRange;
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
			if (agent.remainingDistance <= data.attackRange)
        	{
				Vector3 direction = (Target.position - transform.position).normalized;
				if(direction != Vector3.zero)
				{
					Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
					transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
				}

				if(Target.CompareTag("Base")){
					GameManager.Instance.DamageBase(1);
            		Die();
				}else if(Target.CompareTag("Tower"))
				{
					if(attackTimer <= 0f)
					{
						TowerCombat tower = Target.GetComponent<TowerCombat>();
						if(tower != null)
						{
							tower.TakeDamage(data.attackDamage);
						}
						attackTimer = data.attackRate;
					}
					attackTimer -= Time.deltaTime;
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
			Target = cible;
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