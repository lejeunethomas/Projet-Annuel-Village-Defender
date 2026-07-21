using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    public EnemyData data;

    [SerializeField, Range(0.1f, 2f)]
    private float moveSpeedMultiplier = 0.85f;

    private NavMeshAgent _agent;
    private int _currentHealth;
	private Transform _target;
	private float _attackTimer;
    private bool isDead = false;

    public bool IsDead
    {
        get { return isDead; }
    }

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        
        if (data != null && _agent != null)
        {
            _agent.speed = data.moveSpeed * moveSpeedMultiplier;
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
        if (GameManager.Instance == null ||
            GameManager.Instance.CurrentPhase != GameManager.GamePhase.Wave ||
            isDead ||
            data == null ||
            _agent == null)
        {
            return;
        }

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
            		DieWithoutReward();
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
        if (isDead ||
            GameManager.Instance == null ||
            GameManager.Instance.CurrentPhase != GameManager.GamePhase.Wave)
        {
            return;
        }

        _currentHealth -= damageAmount;
        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    public void KillInstantly()
    {
        if (isDead)
            return;

        Die();
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
        if (isDead)
            return;

        isDead = true;

        if (data != null && GameManager.Instance != null)
            GameManager.Instance.AddGold(data.goldReward);

        if (GameManager.Instance != null)
            GameManager.Instance.UnregisterEnemy();

        Destroy(gameObject);
    }

    void DieWithoutReward()
    {
        if (isDead)
            return;

        isDead = true;

        if (GameManager.Instance != null)
            GameManager.Instance.UnregisterEnemy();

        Destroy(gameObject);
    }

    void OnDestroy() 
    {
        if(TargetManager.Instance != null)
            TargetManager.Instance.ennemisActifs.Remove(this);
    }
}
