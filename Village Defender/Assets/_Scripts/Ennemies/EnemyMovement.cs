using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    public EnemyData data;
    private NavMeshAgent _agent;
    private int _currentHealth;
	private Transform _target;
	private float _attackTimer;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        
        if (data != null)
        {
            _agent.speed = data.moveSpeed;
			_agent.stoppingDistance = data.attackRange;
            _currentHealth = data.maxHealth;
        }

        if(TargetManager.Instance != null)
		{
			TargetManager.Instance.ennemisActifs.Add(this);
		}
    }
    
    void Update()
    {
		if(_target != null && _agent.hasPath && !_agent.pathPending)
		{
			if (_agent.remainingDistance <= data.attackRange)
        	{
				Vector3 direction = (_target.position - transform.position).normalized;
				if(direction != Vector3.zero)
				{
					Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
					transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
				}

				if(_target.CompareTag("Base")){
					GameManager.Instance.DamageBase(1);
            		Die();
				}else if(_target.CompareTag("Tower"))
				{
					if(_attackTimer <= 0f)
					{
						TowerCombat tower = _target.GetComponent<TowerCombat>();
						if(tower != null)
						{
							tower.TakeDamage(data.attackDamage);
						}
						_attackTimer = data.attackRate;
					}
					_attackTimer -= Time.deltaTime;
				}
        	}
		} 
    }

    public void TakeDamage(int damageAmount)
    {
        _currentHealth -= damageAmount;
        if (_currentHealth <= 0)
        {
            if (data != null) GameManager.Instance.AddGold(data.goldReward);
            Die();
        }
    }
    
    public void RecevoirNouvelleCible(Transform cible)
    {
        if (_agent != null && cible != null)
        {
			_target = cible;
            _agent.SetDestination(cible.position);
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